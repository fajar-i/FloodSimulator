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
    
    // Kecepatan menyebar (0.1 = lambat/kental, 0.5 = cepat/encer)
    public float flowSpeed; 

    public void Execute(int i)
    {
        VoxelCell myState = readGrid[i];
        
        // Tembok tidak memproses air
        if (myState.isSolid) {
            writeGrid[i] = myState;
            return;
        }

        // Koordinat Saya
        int x = i % size.x;
        int y = (i / size.x) % size.y;
        int z = i / (size.x * size.y);

        float currentAmount = myState.amount;
        float change = 0f; // Penampung perubahan total frame ini

        // ==========================================
        // 1. LOGIKA VERTIKAL (GRAVITASI)
        // ==========================================
        
        // Cek ATAS (Terima air)
        if (y < size.y - 1) {
            int upIdx = i + size.x;
            VoxelCell upCell = readGrid[upIdx];
            if (!upCell.isSolid && upCell.amount > 0) {
                float space = 1.0f - currentAmount;
                // Air jatuh sangat cepat
                change += math.min(upCell.amount, space); 
            }
        }

        // Cek BAWAH (Buang air)
        if (y > 0) {
            int downIdx = i - size.x;
            VoxelCell downCell = readGrid[downIdx];
            if (!downCell.isSolid) {
                if (downCell.amount < 1.0f) {
                    float spaceBelow = 1.0f - downCell.amount;
                    change -= math.min(currentAmount, spaceBelow);
                }
            }
        }

        // ==========================================
        // 2. LOGIKA HORIZONTAL (MENYEBAR)
        // ==========================================
        
        // Kita definisikan manual arahnya agar hemat memori (tanpa array alokasi)
        // Kiri, Kanan, Belakang, Depan
        int4 dirX = new int4(-1, 1, 0, 0);
        int4 dirZ = new int4(0, 0, -1, 1);

        for (int d = 0; d < 4; d++)
        {
            int nX = x + dirX[d];
            int nZ = z + dirZ[d];

            // Pastikan tetangga masih di dalam grid (tidak keluar map)
            if (nX >= 0 && nX < size.x && nZ >= 0 && nZ < size.z)
            {
                // --- PERBAIKAN RUMUS INDEX DI SINI ---
                int nIdx = nX + (size.x * y) + (size.x * size.y * nZ);
                // -------------------------------------
                
                VoxelCell neighbor = readGrid[nIdx];

                if (!neighbor.isSolid)
                {
                    // Hitung selisih air saya dengan tetangga
                    float diff = currentAmount - neighbor.amount;

                    // Stabilisasi: Hanya alirkan jika selisihnya cukup signifikan
                    if (math.abs(diff) > 0.01f)
                    {
                        // Bagi 4 agar tidak membuang semua air ke satu arah saja
                        float flow = (diff * flowSpeed) / 4.0f;
                        change -= flow; 
                    }
                }
            }
        }

        // ==========================================
        // 3. FINALISASI
        // ==========================================
        
        float finalAmount = currentAmount + change;
        finalAmount = math.saturate(finalAmount); // Kunci angka di antara 0.0 - 1.0

        VoxelCell result = myState;
        result.amount = finalAmount;
        writeGrid[i] = result;
    }
}