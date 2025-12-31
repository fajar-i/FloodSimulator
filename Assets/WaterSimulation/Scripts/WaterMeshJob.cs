using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct WaterMeshJob : IJob
{
    [ReadOnly] public NativeArray<VoxelCell> grid;
    public int3 size;

    // Output data mesh
    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;

    public void Execute()
    {
        // Loop seluruh grid
        for (int i = 0; i < grid.Length; i++)
        {
            VoxelCell cell = grid[i];

            // Skip jika air sangat sedikit atau kosong
            if (cell.amount < 0.01f) continue;

            // Hitung posisi x,y,z dari index
            int x = i % size.x;
            int y = (i / size.x) % size.y;
            int z = i / (size.x * size.y);

            float3 pos = new float3(x, y, z);
            float h = cell.amount; // Tinggi air (0.0 sampai 1.0)

            // Tambahkan Kubus Air
            AddWaterCube(pos, h);
        }
    }

    void AddWaterCube(float3 p, float h)
    {
        int vStart = vertices.Length;

        // Definisi 8 titik sudut kubus
        // p = posisi dasar (x,y,z), h = tinggi air
        // Titik Bawah (y)
        Vector3 b0 = new Vector3(p.x, p.y, p.z);
        Vector3 b1 = new Vector3(p.x + 1, p.y, p.z);
        Vector3 b2 = new Vector3(p.x, p.y, p.z + 1);
        Vector3 b3 = new Vector3(p.x + 1, p.y, p.z + 1);

        // Titik Atas (y + h) -> Air naik turun sesuai amount
        Vector3 t0 = new Vector3(p.x, p.y + h, p.z);
        Vector3 t1 = new Vector3(p.x + 1, p.y + h, p.z);
        Vector3 t2 = new Vector3(p.x, p.y + h, p.z + 1);
        Vector3 t3 = new Vector3(p.x + 1, p.y + h, p.z + 1);

        // Masukkan Vertices (Total 8 titik unik, tapi untuk flat shading kita butuh duplikat per sisi.
        // Untuk sederhananya kita pakai cara "Naive" tanpa sharing vertex agar coding lebih pendek)
        // KITA GENERATE PER SISI (QUADS)
        
        // Top Face (Paling penting untuk dilihat)
        AddQuad(t0, t2, t3, t1);

        // Bottom Face
        AddQuad(b2, b0, b1, b3);

        // North Face
        AddQuad(b2, t2, t0, b0);

        // South Face
        AddQuad(b1, t1, t3, b3);

        // West Face
        AddQuad(b0, t0, t1, b1);

        // East Face
        AddQuad(b3, t3, t2, b2);
    }

    void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Length;
        
        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        // Segitiga 1
        triangles.Add(index + 0);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        // Segitiga 2
        triangles.Add(index + 0);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }
}