using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;

public class ChunkedWaterManager : MonoBehaviour
{
    [Header("World Settings")]
    public int worldWidth = 32;  // Total lebar dunia
    public int worldHeight = 32;
    public int worldDepth = 32;

    [Header("Chunk Settings")]
    public const int CHUNK_SIZE = 16; // Ukuran tetap per chunk
    public Material waterMaterial;

    [Header("Simulation")]
    public float simulationDelay = 0.05f;
    public float flowSpeed = 0.5f;
    [Header("Flood Controls")]
    public bool isFlooding = false;
    [Range(0, 64)] public int targetFloodLevel = 0; // Air akan naik sampai ketinggian Y ini
    public float floodRiseRate = 0.1f; // Seberapa cepat air naik (detik)
    private float floodTimer = 0f;

    // Data Physics (Tetap SATU ARRAY BESAR agar kalkulasi air lancar antar chunk)
    NativeArray<VoxelCell> gridA;
    NativeArray<VoxelCell> gridB;
    bool useGridA = true;
    float tickTimer = 0f;

    // Data Chunks
    WaterChunk[,,] chunks; // Array 3D untuk menyimpan referensi chunk
    int chunksX, chunksY, chunksZ;

    // Buffer Memori untuk Meshing (Dipakai ulang oleh tiap chunk bergantian)
    NativeList<Vector3> sharedVertices;
    NativeList<int> sharedTriangles;

    void Start()
    {
        // 1. Setup Grid Physics Global
        int totalVoxels = worldWidth * worldHeight * worldDepth;
        gridA = new NativeArray<VoxelCell>(totalVoxels, Allocator.Persistent);
        gridB = new NativeArray<VoxelCell>(totalVoxels, Allocator.Persistent);

        // 2. Hitung jumlah chunk yang dibutuhkan
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

        // 4. Siapkan Buffer Mesh Shared
        // Kita pakai satu buffer besar untuk memproses chunk satu per satu
        sharedVertices = new NativeList<Vector3>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 24, Allocator.Persistent);
        sharedTriangles = new NativeList<int>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 36, Allocator.Persistent);

        // Di dalam Update()
        if (isFlooding)
        {
            floodTimer += Time.deltaTime;
            if (floodTimer >= floodRiseRate)
            {
                floodTimer = 0f;
                RiseFlood();
            }
        }

        void RiseFlood()
        {
            // Kita akses Grid secara manual untuk menaikkan air
            // Logikanya: Isi air di seluruh pinggiran peta atau di area "Sungai"

            var grid = useGridA ? gridA : gridB; // Ambil grid aktif

            // Contoh: Banjir datang dari sisi X=0 (Kawasan Perairan)
            int riverWidth = 5; // 5 blok dari pinggir

            for (int x = 0; x < riverWidth; x++)
                for (int z = 0; z < worldDepth; z++)
                    for (int y = 0; y < targetFloodLevel; y++)
                    {
                        int idx = x + worldWidth * (y + worldHeight * z);
                        VoxelCell c = grid[idx];

                        // Hanya isi jika belum penuh solid
                        if (!c.isSolid && c.amount < 1.0f)
                        {
                            c.amount = 1.0f;
                            grid[idx] = c;
                        }
                    }

            // Karena kita mengubah grid secara manual (di luar Job), 
            // perubahan ini akan diproses oleh WaterPullJob di tick berikutnya
            // dan air akan mulai mengalir ke kota.
        }
    }

    void Update()
    {
        // --- INPUT (Sama seperti sebelumnya) ---
        if (Keyboard.current.spaceKey.isPressed)
        {
            NativeArray<VoxelCell> currentGrid = useGridA ? gridA : gridB;
            // Spawn air di tengah-tengah dunia
            int midX = worldWidth / 2;
            int midY = worldHeight - 5;
            int midZ = worldDepth / 2;
            int idx = midX + worldWidth * (midY + worldHeight * midZ);

            VoxelCell c = currentGrid[idx];
            c.amount = 1.0f; c.isSolid = false;
            currentGrid[idx] = c;
        }

        // --- SIMULASI PHYSICS ---
        tickTimer += Time.deltaTime;
        if (tickTimer >= simulationDelay)
        {
            tickTimer -= simulationDelay;
            RunPhysics();

            // Setelah fisika selesai, update visual semua chunk
            UpdateAllChunks();
        }
    }

    void RunPhysics()
    {
        var read = useGridA ? gridA : gridB;
        var write = useGridA ? gridB : gridA;

        var job = new WaterPullJob // Pakai Job Fisika yang sudah Anda punya (yang sudah diperbaiki)
        {
            readGrid = read,
            writeGrid = write,
            size = new int3(worldWidth, worldHeight, worldDepth),
            flowSpeed = flowSpeed
        };

        // Jalankan fisika untuk SELURUH dunia sekaligus (masih efisien)
        job.Schedule(read.Length, 64).Complete();

        useGridA = !useGridA;
    }

    void UpdateAllChunks()
    {
        var gridToDraw = useGridA ? gridA : gridB;

        // Loop semua chunk
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

        // 2. Hitung posisi mulai di Global Grid
        int startX = cx * CHUNK_SIZE;
        int startY = cy * CHUNK_SIZE;
        int startZ = cz * CHUNK_SIZE;

        // 3. Siapkan Job Mesh untuk Chunk ini saja
        var meshJob = new WaterChunkMeshJob
        {
            grid = grid,
            totalGridSize = new int3(worldWidth, worldHeight, worldDepth),
            chunkStartPos = new int3(startX, startY, startZ),
            chunkSize = CHUNK_SIZE,
            vertices = sharedVertices,
            triangles = sharedTriangles
        };

        // 4. Jalankan Job (Single Threaded per chunk sudah cukup cepat karena ukurannya kecil)
        meshJob.Schedule().Complete();

        // 5. Jika ada mesh yang dihasilkan, upload ke GPU
        WaterChunk chunk = chunks[cx, cy, cz];

        // Optimasi: Jika vertex kosong (tidak ada air di chunk ini), kosongkan mesh
        if (sharedVertices.Length == 0)
        {
            chunk.mesh.Clear();
            return;
        }

        chunk.mesh.Clear();
        chunk.mesh.SetVertices(sharedVertices.AsArray());
        chunk.mesh.SetIndices(sharedTriangles.AsArray(), MeshTopology.Triangles, 0);
        chunk.mesh.RecalculateNormals();
        chunk.mesh.RecalculateBounds();
    }

    void OnDestroy()
    {
        if (gridA.IsCreated) gridA.Dispose();
        if (gridB.IsCreated) gridB.Dispose();
        if (sharedVertices.IsCreated) sharedVertices.Dispose();
        if (sharedTriangles.IsCreated) sharedTriangles.Dispose();
    }
}