using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct WaterChunkMeshJob : IJob
{
    [ReadOnly] public NativeArray<VoxelCell> grid;
    public int3 totalGridSize;
    
    public int3 chunkStartPos;
    public int chunkSize;

    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;

    public void Execute()
    {
        for (int x = 0; x < chunkSize; x++)
        for (int y = 0; y < chunkSize; y++)
        for (int z = 0; z < chunkSize; z++)
        {
            int globalX = chunkStartPos.x + x;
            int globalY = chunkStartPos.y + y;
            int globalZ = chunkStartPos.z + z;

            if (globalX >= totalGridSize.x || globalY >= totalGridSize.y || globalZ >= totalGridSize.z) 
                continue;

            int index = GetIndex(globalX, globalY, globalZ);
            VoxelCell cell = grid[index];

            // Skip jika voxel ini kosong
            if (cell.amount < 0.01f) continue;

            float3 pos = new float3(x, y, z);
            float h = cell.amount;

            // --- LOGICA FACE CULLING ---
            // Hanya gambar sisi jika tetangga di arah tersebut KOSONG atau LEBIH RENDAH
            
            // Cek Atas
            if (ShouldDrawFace(globalX, globalY + 1, globalZ, h)) 
            {
                // Top Face logic
                float3 t0 = new float3(pos.x, pos.y + h, pos.z);
                float3 t1 = new float3(pos.x + 1, pos.y + h, pos.z);
                float3 t2 = new float3(pos.x, pos.y + h, pos.z + 1);
                float3 t3 = new float3(pos.x + 1, pos.y + h, pos.z + 1);
                AddQuad(t0, t2, t3, t1);
            }

            // Cek Bawah
            if (ShouldDrawFace(globalX, globalY - 1, globalZ, 1.0f)) // Bawah selalu bandingkan full
            {
                float3 b0 = new float3(pos.x, pos.y, pos.z);
                float3 b1 = new float3(pos.x + 1, pos.y, pos.z);
                float3 b2 = new float3(pos.x, pos.y, pos.z + 1);
                float3 b3 = new float3(pos.x + 1, pos.y, pos.z + 1);
                AddQuad(b2, b0, b1, b3);
            }

            // Cek Kiri (X-1)
            if (ShouldDrawFace(globalX - 1, globalY, globalZ, h))
            {
                // West Face
                AddQuadSide(pos, h, 0); 
            }
            
            // Cek Kanan (X+1)
            if (ShouldDrawFace(globalX + 1, globalY, globalZ, h))
            {
                // East Face
                AddQuadSide(pos, h, 1);
            }

            // Cek Belakang (Z-1)
            if (ShouldDrawFace(globalX, globalY, globalZ - 1, h))
            {
                // South Face
                AddQuadSide(pos, h, 2);
            }

            // Cek Depan (Z+1)
            if (ShouldDrawFace(globalX, globalY, globalZ + 1, h))
            {
                // North Face
                AddQuadSide(pos, h, 3);
            }
        }
    }

    // Helper untuk mengecek apakah kita perlu menggambar wajah
    bool ShouldDrawFace(int gx, int gy, int gz, float myHeight)
    {
        // 1. Jika tetangga di luar map, gambar sisi ini (batas dunia)
        if (gx < 0 || gx >= totalGridSize.x || 
            gy < 0 || gy >= totalGridSize.y || 
            gz < 0 || gz >= totalGridSize.z)
            return true;

        // 2. Ambil data tetangga
        int idx = GetIndex(gx, gy, gz);
        VoxelCell neighbor = grid[idx];

        // 3. Jika tetangga solid (tembok), jangan gambar (hemat GPU)
        if (neighbor.isSolid) return false;

        // 4. Jika tetangga airnya lebih tinggi atau sama penuhnya, jangan gambar
        // (Kecuali saya penuh 1.0, tetangga 0.5, saya tetap perlu gambar dinding saya yang terekspos)
        // Aturan simpel: Gambar jika tetangga kurang dari penuh
        if (neighbor.amount >= 0.99f) return false;
        
        // Optimasi tambahan: Jika tinggi saya sama persis dengan tetangga, bisa di-skip
        // tapi untuk amannya return true jika tetangga tidak penuh.
        return true;
    }

    int GetIndex(int x, int y, int z)
    {
        return x + totalGridSize.x * (y + totalGridSize.y * z);
    }

    // Helper simplifikasi Quad Samping
    void AddQuadSide(float3 p, float h, int side)
    {
        // Titik dasar
        float3 b0 = new float3(p.x, p.y, p.z);
        float3 b1 = new float3(p.x + 1, p.y, p.z);
        float3 b2 = new float3(p.x, p.y, p.z + 1);
        float3 b3 = new float3(p.x + 1, p.y, p.z + 1);

        // Titik atas
        float3 t0 = new float3(p.x, p.y + h, p.z);
        float3 t1 = new float3(p.x + 1, p.y + h, p.z);
        float3 t2 = new float3(p.x, p.y + h, p.z + 1);
        float3 t3 = new float3(p.x + 1, p.y + h, p.z + 1);

        if (side == 2) AddQuad(b0, t0, t1, b1); // South
        else if (side == 3) AddQuad(b3, t3, t2, b2); // North
        else if (side == 1) AddQuad(b1, t1, t3, b3); // East
        else if (side == 0) AddQuad(b2, t2, t0, b0); // West
    }

    void AddQuad(float3 v0, float3 v1, float3 v2, float3 v3)
    {
        int index = vertices.Length;
        vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
        triangles.Add(index + 0); triangles.Add(index + 1); triangles.Add(index + 2);
        triangles.Add(index + 0); triangles.Add(index + 2); triangles.Add(index + 3);
    }
}