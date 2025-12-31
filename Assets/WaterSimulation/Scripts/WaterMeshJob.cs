using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct WaterChunkMeshJob : IJob
{
    // --- INPUT DATA ---
    [ReadOnly] public NativeArray<VoxelCell> grid;
    public int3 totalGridSize;
    public int3 chunkStartPos;
    public int chunkSize;

    // --- OUTPUT DATA ---
    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector2> uvs;

    // Konstanta Texture Atlas (Misal 4x4 grid)
    private const float uvStep = 0.25f; // 1.0 dibagi 4
    private const int atlasCols = 4;
    private const float uvEps = 0.001f; // Margin agar tidak bocor warna

    public void Execute()
    {
        // Loop 3D Voxel
        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
                for (int z = 0; z < chunkSize; z++)
                {
                    // 1. Koordinat Global
                    int gx = chunkStartPos.x + x;
                    int gy = chunkStartPos.y + y;
                    int gz = chunkStartPos.z + z;

                    // Safety Check
                    if (gx >= totalGridSize.x || gy >= totalGridSize.y || gz >= totalGridSize.z) continue;

                    // 2. Ambil Data Voxel Ini
                    int index = GetIndex(gx, gy, gz);
                    VoxelCell cell = grid[index];

                    // Skip jika kosong
                    if (cell.amount < 0.01f && !cell.isSolid) continue;

                    float3 localPos = new float3(x, y, z);
                    float height = cell.isSolid ? 1.0f : cell.amount; // Solid selalu penuh
                    int type = cell.blockType;

                    // 3. CEK 6 SISI (Face Culling)
                    // Kita hanya menggambar sisi jika tetangga di arah itu transparan/lebih rendah

                    // ATAS (Y+1)
                    if (ShouldDrawFace(gx, gy + 1, gz, height))
                        AddFace(localPos, height, type, 0); // 0 = Atas

                    // BAWAH (Y-1)
                    if (ShouldDrawFace(gx, gy - 1, gz, 1.0f)) // Bawah selalu cek full block
                        AddFace(localPos, height, type, 1); // 1 = Bawah

                    // KIRI (X-1)
                    if (ShouldDrawFace(gx - 1, gy, gz, height))
                        AddFace(localPos, height, type, 2); // 2 = Kiri (West)

                    // KANAN (X+1)
                    if (ShouldDrawFace(gx + 1, gy, gz, height))
                        AddFace(localPos, height, type, 3); // 3 = Kanan (East)

                    // BELAKANG (Z-1)
                    if (ShouldDrawFace(gx, gy, gz - 1, height))
                        AddFace(localPos, height, type, 4); // 4 = Belakang (South)

                    // DEPAN (Z+1)
                    if (ShouldDrawFace(gx, gy, gz + 1, height))
                        AddFace(localPos, height, type, 5); // 5 = Depan (North)
                }
    }

    // --- FUNGSI 1: LOGIKA FACE CULLING ---
    // Mengembalikan TRUE jika wajah harus digambar (karena terekspos)
    bool ShouldDrawFace(int gx, int gy, int gz, float myHeight)
    {
        // A. Jika tetangga di luar peta -> Gambar (Batas dunia)
        if (gx < 0 || gx >= totalGridSize.x ||
            gy < 0 || gy >= totalGridSize.y ||
            gz < 0 || gz >= totalGridSize.z)
            return true;

        // B. Ambil tetangga
        int idx = GetIndex(gx, gy, gz);
        VoxelCell neighbor = grid[idx];

        // C. Jika tetangga Solid (Tembok/Tanah) -> Jangan Gambar (Ketutup tembok)
        if (neighbor.isSolid) return false;

        // D. Jika tetangga Air
        // Gambar hanya jika air tetangga lebih rendah dari saya
        // (Atau tetangga kosong/amount=0)
        return neighbor.amount < myHeight;
    }

    // --- FUNGSI 2: GEOMETRI & UV GENERATOR ---
    // Menambahkan 4 titik (Quad) dan UV map untuk satu sisi spesifik
    // --- FUNGSI 2: GEOMETRI & UV GENERATOR ---
    void AddFace(float3 p, float h, int type, int faceDir)
    {
        int vStart = vertices.Length;

        // --- DEFINISI TITIK SUDUT (Tetap Sama) ---
        // b = Bottom, t = Top
        // 0 = Kiri Bawah/Belakang (0,0)
        // 1 = Kanan Bawah/Belakang (1,0)
        // 2 = Kiri Atas/Depan (0,1)
        // 3 = Kanan Atas/Depan (1,1) (Relatif XZ plane)

        float3 b0 = new float3(p.x, p.y, p.z);         // 0,0,0
        float3 b1 = new float3(p.x + 1, p.y, p.z);     // 1,0,0
        float3 b2 = new float3(p.x, p.y, p.z + 1);     // 0,0,1
        float3 b3 = new float3(p.x + 1, p.y, p.z + 1); // 1,0,1

        float3 t0 = new float3(p.x, p.y + h, p.z);     // 0,h,0
        float3 t1 = new float3(p.x + 1, p.y + h, p.z); // 1,h,0
        float3 t2 = new float3(p.x, p.y + h, p.z + 1); // 0,h,1
        float3 t3 = new float3(p.x + 1, p.y + h, p.z + 1); // 1,h,1

        // --- PERBAIKAN URUTAN TITIK (WINDING ORDER) ---
        // Urutan harus: Bawah-Kiri -> Atas-Kiri -> Atas-Kanan -> Bawah-Kanan
        // Dilihat DARI LUAR kubus menghadap ke wajah tersebut.

        switch (faceDir)
        {
            case 0: // Top (Y+1) - OK
                // Melihat dari atas ke bawah
                vertices.Add(t0); vertices.Add(t2); vertices.Add(t3); vertices.Add(t1);
                break;

            case 1: // Bottom (Y-1) - OK
                // Melihat dari bawah ke atas
                vertices.Add(b2); vertices.Add(b0); vertices.Add(b1); vertices.Add(b3);
                break;

            case 2: // Left / West (X-1) - DIPERBAIKI
                // Melihat dari kiri ke kanan (Lihat ke arah X positif)
                // Urutan: Belakang-Bawah (b2) -> Belakang-Atas (t2) -> Depan-Atas (t0) -> Depan-Bawah (b0)
                vertices.Add(b2); vertices.Add(t2); vertices.Add(t0); vertices.Add(b0);
                break;

            case 3: // Right / East (X+1) - DIPERBAIKI
                // Melihat dari kanan ke kiri (Lihat ke arah X negatif)
                vertices.Add(b1); vertices.Add(t1); vertices.Add(t3); vertices.Add(b3);
                break;

            case 4: // Back / South (Z-1) - DIPERBAIKI
                // Melihat dari belakang ke depan (Lihat ke arah Z positif)
                vertices.Add(b0); vertices.Add(t0); vertices.Add(t1); vertices.Add(b1);
                break;

            case 5: // Front / North (Z+1) - DIPERBAIKI
                // Melihat dari depan ke belakang (Lihat ke arah Z negatif)
                vertices.Add(b3); vertices.Add(t3); vertices.Add(t2); vertices.Add(b2);
                break;
        }

        // --- TRIANGLES (Tetap Sama) ---
        // 0-1-2 dan 0-2-3 membentuk kotak dari 4 titik yang sudah diurutkan di atas
        triangles.Add(vStart + 0);
        triangles.Add(vStart + 1);
        triangles.Add(vStart + 2);

        triangles.Add(vStart + 0);
        triangles.Add(vStart + 2);
        triangles.Add(vStart + 3);

        // --- UV MAPPING (Tetap Sama) ---
        float uvX = (type % atlasCols) * uvStep;
        float uvY = (type / atlasCols) * uvStep;

        Vector2 u0 = new Vector2(uvX + uvEps, uvY + uvEps);
        Vector2 u1 = new Vector2(uvX + uvEps, uvY + uvStep - uvEps);
        Vector2 u2 = new Vector2(uvX + uvStep - uvEps, uvY + uvStep - uvEps);
        Vector2 u3 = new Vector2(uvX + uvStep - uvEps, uvY + uvEps);

        uvs.Add(u0); uvs.Add(u1); uvs.Add(u2); uvs.Add(u3);
    }

    // Helper hitung index 1D
    int GetIndex(int x, int y, int z)
    {
        return x + totalGridSize.x * (y + totalGridSize.y * z);
    }
}