using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;

public class WaterManager : MonoBehaviour
{
    [Header("World Settings")]
    public int worldWidth = 32;
    public int worldHeight = 32;
    public int worldDepth = 32;

    [Header("Chunk Settings")]
    public const int CHUNK_SIZE = 16;
    public Material waterMaterial;

    [Header("Simulation")]
    public float simulationDelay = 0.05f;
    public float flowSpeed = 0.5f;

    [Header("Flood Controls")]
    public bool isFlooding = false;
    [Range(0, 64)] public int targetFloodLevel = 10;
    public float floodRiseRate = 0.5f;
    private float floodTimer = 0f;

    // Data Physics (Public agar bisa diakses Generator)
    public NativeArray<VoxelCell> gridA;
    NativeArray<VoxelCell> gridB;
    bool useGridA = true;
    float tickTimer = 0f;

    // Data Chunks
    WaterChunk[,,] chunks;
    int chunksX, chunksY, chunksZ;

    // Buffer Memori Shared (Vertices, Triangles, UVs)
    NativeList<Vector3> sharedVertices;
    NativeList<int> sharedTriangles;
    NativeList<Vector2> sharedUVs; // <--- Tambahan untuk Texture

    void Awake()
    {
        // 1. Setup Grid Physics Global
        int totalVoxels = worldWidth * worldHeight * worldDepth;
        gridA = new NativeArray<VoxelCell>(totalVoxels, Allocator.Persistent);
        gridB = new NativeArray<VoxelCell>(totalVoxels, Allocator.Persistent);

        // 2. Hitung jumlah chunk
        chunksX = Mathf.CeilToInt(worldWidth / (float)CHUNK_SIZE);
        chunksY = Mathf.CeilToInt(worldHeight / (float)CHUNK_SIZE);
        chunksZ = Mathf.CeilToInt(worldDepth / (float)CHUNK_SIZE);

        chunks = new WaterChunk[chunksX, chunksY, chunksZ];

        // 3. Spawn Chunk Objects
        for (int x = 0; x < chunksX; x++)
        for (int y = 0; y < chunksY; y++)
        for (int z = 0; z < chunksZ; z++)
        {
            chunks[x, y, z] = new WaterChunk(new Vector3Int(x, y, z), waterMaterial, transform);
        }

        // 4. Siapkan Buffer Mesh Shared (Termasuk UV)
        int maxVertsPerChunk = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 24;
        sharedVertices = new NativeList<Vector3>(maxVertsPerChunk, Allocator.Persistent);
        sharedTriangles = new NativeList<int>(maxVertsPerChunk * 2, Allocator.Persistent); // Estimasi kasar
        sharedUVs = new NativeList<Vector2>(maxVertsPerChunk, Allocator.Persistent); // <--- Inisialisasi UV
    } // <--- Tutup Awake di sini! Jangan lanjut ke bawah.

    void Update()
    {
        // --- INPUT SPAWN AIR MANUAL ---
        if (Keyboard.current.spaceKey.isPressed)
        {
            NativeArray<VoxelCell> currentGrid = useGridA ? gridA : gridB;
            int midX = worldWidth / 2;
            int midY = worldHeight - 5;
            int midZ = worldDepth / 2;
            int idx = midX + worldWidth * (midY + worldHeight * midZ);

            if (idx >= 0 && idx < currentGrid.Length)
            {
                VoxelCell c = currentGrid[idx];
                c.amount = 1.0f; c.isSolid = false; c.blockType = 0; // Tipe Air
                currentGrid[idx] = c;
            }
        }

        // --- LOGIKA BANJIR ---
        if (isFlooding)
        {
            floodTimer += Time.deltaTime;
            if (floodTimer >= floodRiseRate)
            {
                floodTimer = 0f;
                RiseFlood();
            }
        }

        // --- SIMULASI PHYSICS ---
        tickTimer += Time.deltaTime;
        if (tickTimer >= simulationDelay)
        {
            tickTimer -= simulationDelay;
            RunPhysics();
            UpdateAllChunks();
        }
    }

    void RiseFlood()
    {
        var grid = useGridA ? gridA : gridB;
        
        // Banjir dari sisi X=0 sampai X=5 (Sungai meluap)
        int riverWidth = 5; 

        for (int x = 0; x < riverWidth; x++)
        for (int z = 0; z < worldDepth; z++)
        for (int y = 0; y < targetFloodLevel; y++)
        {
            int idx = x + worldWidth * (y + worldHeight * z);
            if (idx >= grid.Length) continue;

            VoxelCell c = grid[idx];
            // Hanya isi jika belum solid
            if (!c.isSolid && c.amount < 1.0f)
            {
                c.amount = 1.0f;
                c.blockType = 0; // Pastikan tipenya Air
                grid[idx] = c;
            }
        }
    }

    void RunPhysics()
    {
        var read = useGridA ? gridA : gridB;
        var write = useGridA ? gridB : gridA;

        var job = new WaterPullJob
        {
            readGrid = read,
            writeGrid = write,
            size = new int3(worldWidth, worldHeight, worldDepth),
            flowSpeed = flowSpeed
        };

        job.Schedule(read.Length, 64).Complete();
        useGridA = !useGridA;
    }

    void UpdateAllChunks()
    {
        var gridToDraw = useGridA ? gridA : gridB;

        for (int x = 0; x < chunksX; x++)
        for (int y = 0; y < chunksY; y++)
        for (int z = 0; z < chunksZ; z++)
        {
            UpdateSingleChunk(x, y, z, gridToDraw);
        }
    }

    void UpdateSingleChunk(int cx, int cy, int cz, NativeArray<VoxelCell> grid)
    {
        // 1. Bersihkan buffer shared
        sharedVertices.Clear();
        sharedTriangles.Clear();
        sharedUVs.Clear(); // <--- Bersihkan UV juga

        // 2. Setup Job Mesh
        var meshJob = new WaterChunkMeshJob
        {
            grid = grid,
            totalGridSize = new int3(worldWidth, worldHeight, worldDepth),
            chunkStartPos = new int3(cx * CHUNK_SIZE, cy * CHUNK_SIZE, cz * CHUNK_SIZE),
            chunkSize = CHUNK_SIZE,
            vertices = sharedVertices,
            triangles = sharedTriangles,
            uvs = sharedUVs // <--- Masukkan List UV
        };

        // 3. Jalankan Job
        meshJob.Schedule().Complete();

        // 4. Update Unity Mesh
        WaterChunk chunk = chunks[cx, cy, cz];

        if (sharedVertices.Length == 0)
        {
            chunk.mesh.Clear();
            return;
        }

        chunk.mesh.Clear();
        chunk.mesh.SetVertices(sharedVertices.AsArray());
        chunk.mesh.SetUVs(0, sharedUVs.AsArray()); // <--- Upload UV ke GPU (Channel 0)
        chunk.mesh.SetIndices(sharedTriangles.AsArray(), MeshTopology.Triangles, 0);
        
        chunk.mesh.RecalculateNormals();
        chunk.mesh.RecalculateBounds();
    }

    void OnDestroy()
    {
        if (gridA.IsCreated) gridA.Dispose();
        if (gridB.IsCreated) gridB.Dispose();
        
        // Dispose Shared Buffers
        if (sharedVertices.IsCreated) sharedVertices.Dispose();
        if (sharedTriangles.IsCreated) sharedTriangles.Dispose();
        if (sharedUVs.IsCreated) sharedUVs.Dispose(); // <--- Jangan lupa dispose UV
    }
}