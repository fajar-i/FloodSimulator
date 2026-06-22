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


    private VoxelWorld world;

    // Dipanggil oleh GameManager setiap frame 
    public void SystemUpdate(VoxelWorld connectedWorld)
    {
        world = connectedWorld;
        if (Keyboard.current.spaceKey.isPressed)
        {
            for (int i = 0; i < 10; i++)
            {

                NativeArray<VoxelCell> currentGrid = world.ActiveGrid;
                // Di SystemUpdate
                int midX = UnityEngine.Random.Range(0, world.worldWidth);
                int midY = world.worldHeight - 5;
                int midZ = UnityEngine.Random.Range(0, world.worldDepth);

                // Rumus Indexing yang Konsisten dengan Job Execute Anda:
                // x + (width * y) + (width * height * z)
                int idx = midX + (world.worldWidth * midY) + (world.worldWidth * world.worldHeight * midZ);

                if (idx >= 0 && idx < currentGrid.Length)
                {
                    VoxelCell c = currentGrid[idx];
                    c.amount = 1.0f; c.isSolid = false; c.blockType = VoxelID.WATER; // Tipe Air
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