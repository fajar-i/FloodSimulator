using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;

public class VoxelWorld : MonoBehaviour
{
    [Header("World Settings")]
    public int worldWidth = 32;
    public int worldHeight = 32;
    public int worldDepth = 32;
    public const int CHUNK_SIZE = 16;
    public Material waterMaterial;

    // Data Physics (Public agar bisa diakses Generator)
    public NativeArray<VoxelCell> ActiveGrid; // Grid A
    public NativeArray<VoxelCell> NextGrid;   // Grid B

    // Data Chunks
    WaterChunk[,,] chunks;
    int chunksX, chunksY, chunksZ;
    // Buffer Memori Shared (Vertices, Triangles, UVs)
    NativeList<Vector3> sharedVertices;
    NativeList<int> sharedTriangles;
    NativeList<Vector2> sharedUVs; // <--- Tambahan untuk Texture

    public void InitializeWorld()
    {
        // 1. Setup Grid Physics Global
        int totalVoxels = worldWidth * worldHeight * worldDepth;
        ActiveGrid = new NativeArray<VoxelCell>(totalVoxels, Allocator.Persistent);
        NextGrid = new NativeArray<VoxelCell>(totalVoxels, Allocator.Persistent);

        SetupChunksAndBuffers();
    }
    void SetupChunksAndBuffers()
    {
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
    }
    public void UpdateAllChunks()
    {
        var gridToDraw = ActiveGrid;

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

        chunk.meshCollider.sharedMesh = null; // Reset dulu agar refresh
        chunk.meshCollider.sharedMesh = chunk.mesh;
    }

    bool IsValidIndex(int x, int y, int z)
    {
        return x >= 0 && x < worldWidth &&
         y >= 0 && y < worldHeight &&
          z >= 0 && z < worldDepth;
    }

    int GetIndex(int x, int y, int z)
    {
        return x + worldWidth * (y + worldHeight * z);
    }
    public VoxelCell GetVoxel(int x, int y, int z)
    {
        // Validasi batas array agar tidak error IndexOutOfRange
        if (!IsValidIndex(x, y, z)) return new VoxelCell();

        int idx = GetIndex(x, y, z);
        return ActiveGrid[idx];
    }

    public void SetVoxel(int x, int y, int z, VoxelCell data)
    {
        if (!IsValidIndex(x, y, z)) return;

        int idx = GetIndex(x, y, z);

        // Cek apakah data benar-benar berubah? (Optimasi)
        // Kalau datanya sama persis, jangan render ulang (Hemat CPU)
        VoxelCell oldData = ActiveGrid[idx];
        if (oldData.blockType == data.blockType && oldData.amount == data.amount && oldData.isSolid == data.isSolid)
            return;

        ActiveGrid[idx] = data;
        NextGrid[idx] = data;

        // OTOMATIS MARK DIRTY DI SINI
        MarkChunkDirty(x, y, z);
    }

    public void MarkChunkDirty(int x, int y, int z)
    {
        // Cek batas dunia dulu agar tidak error
        if (!IsValidIndex(x, y, z)) return;

        // Gunakan variabel lokal (cx, cy, cz) BUKAN global (chunksX)
        int cx = x / CHUNK_SIZE;
        int cy = y / CHUNK_SIZE;
        int cz = z / CHUNK_SIZE;

        UpdateSingleChunk(cx, cy, cz, ActiveGrid);

        // OPTIMASI TAMBAHAN:
        // Jika kita mengubah blok di perbatasan chunk (misal x=15),
        // chunk sebelah (x=16) juga perlu update karena Face Culling tetangga berubah.
        // Tapi untuk sekarang, kode di atas sudah cukup agar tekstur berubah.
    }

    public void SwapBuffer()
    {
        (NextGrid, ActiveGrid) = (ActiveGrid, NextGrid);
    }
    void OnDestroy()
    {
        if (ActiveGrid.IsCreated) ActiveGrid.Dispose();
        if (NextGrid.IsCreated) NextGrid.Dispose();

        // Dispose Shared Buffers
        if (sharedVertices.IsCreated) sharedVertices.Dispose();
        if (sharedTriangles.IsCreated) sharedTriangles.Dispose();
        if (sharedUVs.IsCreated) sharedUVs.Dispose(); // <--- Jangan lupa dispose UV
    }
}