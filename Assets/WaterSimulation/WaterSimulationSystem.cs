using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class WaterSimulationSystem : MonoBehaviour
{
    [Header("Settings")]
    public float simulationDelay = 1f;
    public float flowSpeed = 0.05f;
    [Header("speedup (0-8)")]
    public byte speedUpBy = 1;

    [Header("Flood Controls")]
    public bool isFlooding = false;
    public int targetFloodLevel = 10;
    public float floodRiseRate = 0.05f;
    private float floodTimer = 0f;
    private float tickTimer = 0f;

    [Header("Hujan (dipicu Cuaca)")]
    [Tooltip("Sumber cuaca. Hujan/Badai akan menurunkan air dari atas secara bertahap.")]
    [SerializeField] private EconomyManager economy;
    [Tooltip("Jeda antar tetesan hujan (detik). Makin kecil makin sering.")]
    public float rainInterval = 0.2f;
    [Tooltip("Jumlah kolom yang ditetesi tiap tick saat cuaca Hujan.")]
    public int hujanDrops = 6;
    [Tooltip("Jumlah kolom yang ditetesi tiap tick saat cuaca Badai.")]
    public int badaiDrops = 18;
    [Tooltip("Banyak air per tetes (0-1). Air menumpuk pelan sampai 1.0 per sel.")]
    public float rainAmount = 0.25f;
    private float rainTimer = 0f;


    private VoxelWorld world;

    // Dipanggil oleh GameManager setiap frame 
    public void SystemUpdate(VoxelWorld connectedWorld)
    {
        world = connectedWorld;
        if (Keyboard.current.spaceKey.isPressed)
        {
            for (int i = 0; i < 5; i++)
            {

                NativeArray<VoxelCell> currentGrid = world.ActiveGrid;
                // Di SystemUpdate
                int midX = UnityEngine.Random.Range(0, world.worldWidth);
                int midY = UnityEngine.Random.Range(world.worldHeight - 2, world.worldHeight);
                int midZ = UnityEngine.Random.Range(0, world.worldDepth);

                // Rumus Indexing yang Konsisten dengan Job Execute Anda:
                // x + (width * y) + (width * height * z)
                int idx = midX + (world.worldWidth * midY) + (world.worldWidth * world.worldHeight * midZ);

                if (idx >= 0 && idx < currentGrid.Length)
                {
                    VoxelCell c = currentGrid[idx];
                    c.amount = 0.1f; c.isSolid = false; c.blockType = VoxelID.WATER; // Tipe Air
                    currentGrid[idx] = c;
                }
            }
        }
        if (isFlooding)
        {
            floodTimer += Time.deltaTime;
            if (floodTimer >= floodRiseRate)
            {
                floodTimer = 0f;
                RiseFloodLogic();
            }
        }

        // Hujan otomatis: air turun dari atas & menggenang, intensitas ikut cuaca.
        UpdateRain();

        //logika fisika air
        tickTimer += Time.deltaTime;
        if (tickTimer >= simulationDelay)
        {
            tickTimer -= simulationDelay;
            for (int i = 0; i < speedUpBy; i++)
            {
                RunPhysicStep();
            }
            // Setelah fisika selesai, suruh World render ulang
            world.UpdateAllChunks();
        }
    }
    void RunPhysicStep()
    {
        var read = world.ActiveGrid;
        var write = world.NextGrid;

        var job = new WaterPullJob
        {
            readGrid = read,
            writeGrid = write,
            size = new int3(world.worldWidth, world.worldHeight, world.worldDepth),
            flowSpeed = flowSpeed
        };
        job.Schedule(read.Length, 64).Complete();
        world.SwapBuffer();
    }
    // Hujan dari atas: tiap tick, sejumlah kolom acak ditetesi air tepat di atas
    // permukaannya (terrain/air teratas). CA lalu mengalirkannya turun & menyamping,
    // sehingga air menggenang di cekungan dan level naik PERLAHAN (bukan sesaat).
    void UpdateRain()
    {
        if (economy == null) return;

        int drops;
        switch (economy.Weather)
        {
            case EconomyManager.WeatherType.Hujan: drops = hujanDrops; break;
            case EconomyManager.WeatherType.Badai: drops = badaiDrops; break;
            default: rainTimer = 0f; return; // Cerah: tidak hujan
        }

        rainTimer += Time.deltaTime;
        if (rainTimer < rainInterval) return;
        rainTimer = 0f;

        var grid = world.ActiveGrid;
        int w = world.worldWidth, h = world.worldHeight, d = world.worldDepth;

        for (int n = 0; n < drops; n++)
        {
            int x = UnityEngine.Random.Range(0, w);
            int z = UnityEngine.Random.Range(0, d);

            // Cari permukaan: y tertinggi yang sudah terisi (solid / sudah ada air).
            int surfaceY = 0;
            for (int y = 0; y < h; y++)
            {
                VoxelCell cc = grid[x + w * (y + h * z)];
                if (cc.isSolid || cc.amount > 0.1f) surfaceY = y + 1;
            }
            if (surfaceY >= h) surfaceY = h - 1; // sudah penuh sampai atas

            int idx = x + w * (surfaceY + h * z);
            VoxelCell c = grid[idx];
            if (!c.isSolid)
            {
                c.amount = Mathf.Min(1.0f, c.amount + rainAmount);
                c.blockType = VoxelID.WATER;
                grid[idx] = c;
            }
        }
    }

    void RiseFloodLogic()
    {
        var grid = world.ActiveGrid;

        // Banjir dari sisi X=0 sampai X=5 (Sungai meluap)
        int riverWidth = 5;

        for (int x = 0; x < riverWidth; x++)
            for (int z = 0; z < world.worldDepth; z++)
                for (int y = 0; y < targetFloodLevel; y++)
                {
                    int idx = x + world.worldWidth * (y + world.worldHeight * z);
                    if (idx >= grid.Length) continue;

                    VoxelCell c = grid[idx];
                    // Hanya isi jika belum solid
                    if (!c.isSolid && c.amount < 1.0f)
                    {
                        c.amount = 1.0f;
                        c.blockType = VoxelID.WATER; // Pastikan tipenya Air
                        grid[idx] = c;
                    }
                }
    }


}