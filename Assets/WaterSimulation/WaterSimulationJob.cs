using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct WaterPullJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<VoxelCell> readGrid;
    public NativeArray<VoxelCell> writeGrid;
    public int3 size;

    // Kecepatan menyebar dasar
    public float flowSpeed;

    // Ambang batas air tergenang (Depression Storage)
    // Air di bawah level ini dianggap terjebak di cekungan mikro dan tidak mengalir lateral
    private const float DEPRESSION_STORAGE = 0.05f;

    private bool CanFlowDown(int cellIdx, int cellY)
    {
        if (cellY <= 0) return false;
        int downIdx = cellIdx - size.x;
        VoxelCell downCell = readGrid[downIdx];
        return !downCell.isSolid && downCell.amount < 0.99f;
    }

    public void Execute(int i)
    {
        VoxelCell myState = readGrid[i];

        // Tembok tidak memproses air
        if (myState.isSolid)
        {
            writeGrid[i] = myState;
            return;
        }

        // Koordinat Saya
        int x = i % size.x;
        int y = (i / size.x) % size.y;
        int z = i / (size.x * size.y);

        float currentAmount = myState.amount;
        float change = 0f;

        // ==========================================
        // 1. LOGIKA VERTIKAL (GRAVITASI)
        // ==========================================
        // (Tidak berubah, logika vertikal Anda sudah efisien)

        // Cek ATAS (Terima air)
        if (y < size.y - 1)
        {
            int upIdx = i + size.x;
            VoxelCell upCell = readGrid[upIdx];
            if (!upCell.isSolid && upCell.amount > 0)
            {
                float space = 1.0f - currentAmount;
                change += math.min(upCell.amount, space);
            }
        }

        // Cek BAWAH (Buang air)
        if (y > 0)
        {
            int downIdx = i - size.x;
            VoxelCell downCell = readGrid[downIdx];
            if (!downCell.isSolid)
            {
                if (downCell.amount < 1.0f)
                {
                    float spaceBelow = 1.0f - downCell.amount;
                    change -= math.min(currentAmount, spaceBelow);
                }
            }
        }

        // ==========================================
        // 2. LOGIKA HORIZONTAL (CA-DUSRM ADAPTED)
        // ==========================================

        if (!CanFlowDown(i, y))
        {
            // Tentukan faktor kekasaran SAYA (Sender Roughness) secara dinamis
            // flowFriction = 0.0f (licin total/cepat) -> myRoughness = 1.0f
            // flowFriction = 0.7f (kasar/tanah) -> myRoughness = 0.3f
            float myRoughness = 1.0f - myState.flowFriction;

            int4 dirX = new int4(-1, 1, 0, 0);
            int4 dirZ = new int4(0, 0, -1, 1);

            for (int d = 0; d < 4; d++)
            {
                int nX = x + dirX[d];
                int nZ = z + dirZ[d];

                if (nX >= 0 && nX < size.x && nZ >= 0 && nZ < size.z)
                {
                    int nIdx = nX + (size.x * y) + (size.x * size.y * nZ);

                    VoxelCell neighbor = readGrid[nIdx];

                    if (!neighbor.isSolid)
                    {
                        // ... di dalam loop for 4 arah ...

                        // Hitung perbedaan air
                        float diff = currentAmount - neighbor.amount;

                        if (math.abs(diff) > 0.01f)
                        {
                            // KASUS A: OUTFLOW (Saya -> Tetangga)
                            if (diff > 0)
                            {
                                if (currentAmount > DEPRESSION_STORAGE)
                                {
                                    // 1. Hitung keinginan aliran (Desired Flow)
                                    float desiredFlow = (diff * flowSpeed * myRoughness) * 0.25f;

                                    // 2. [FIX] Batasi aliran! 
                                    // Kita tidak boleh memberi lebih dari 1/4 air yang kita miliki per arah
                                    // atau melebihi diff/4 (agar tidak overshoot/bolak-balik)
                                    float maxFlowPossible = currentAmount * 0.25f;

                                    // Pilih yang paling kecil agar aman
                                    float actualFlow = math.min(desiredFlow, maxFlowPossible);

                                    change -= actualFlow;
                                }
                            }
                            // KASUS B: INFLOW (Tetangga -> Saya)
                            else
                            {
                                if (neighbor.amount > DEPRESSION_STORAGE)
                                {
                                    float neighborRoughness = 1.0f - neighbor.flowFriction;

                                    // 1. Hitung keinginan aliran (Note: diff negatif, jadi flow negatif)
                                    float desiredFlow = (diff * flowSpeed * neighborRoughness) * 0.25f;

                                    // 2. [FIX] Batasi aliran masuk!
                                    // Tetangga tidak bisa memberi lebih dari 1/4 air yang DIA miliki
                                    float maxInflowPossible = neighbor.amount * 0.25f;

                                    // math.max karena angkanya negatif (misal: max(-2.5, -0.25) = -0.25)
                                    float actualFlow = math.max(desiredFlow, -maxInflowPossible);

                                    change -= actualFlow; // Minus ketemu minus jadi plus
                                }
                            }
                        }
                    }
                }
            }
        }

        // ==========================================
        // 3. FINALISASI
        // ==========================================

        float finalAmount = currentAmount + change;

        // Penyerapan air (absorptionRate) per tick
        if (finalAmount > 0.0f && myState.absorptionRate > 0.0f)
        {
            finalAmount -= myState.absorptionRate * flowSpeed;
            if (finalAmount < 0.0f) finalAmount = 0.0f;
        }

        finalAmount = math.saturate(finalAmount);

        VoxelCell result = myState;
        result.amount = finalAmount;
        writeGrid[i] = result;
    }
}