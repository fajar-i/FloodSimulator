using UnityEngine;
using Unity.Collections;

public class CityTerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public int seed = 12345;
    public bool generateOnStart = false;
    public int groundBaseHeight = 5; // Tinggi dasar tanah

    [Header("River Settings")]
    public int riverWidth = 6;
    public float riverMeanderScale = 20f; // Seberapa bergelombang sungainya
    public float riverMeanderAmplitude = 15f; // Seberapa jauh belokannya

    [Header("Building Settings")]
    [Range(0, 1)] public float buildingDensity = 0.3f;
    public int maxBuildingHeight = 20;

    public void GenerateCity(VoxelWorld voxelWorld)
    {
        Random.InitState(seed);
        // Kita butuh akses langsung ke Grid dari Manager
        GenerateTerrainData(voxelWorld);
    }

    void GenerateTerrainData(VoxelWorld voxelWorld)
    {
        int width = voxelWorld.worldWidth;
        int height = voxelWorld.worldHeight;
        int depth = voxelWorld.worldDepth;

        // Kita akan mengisi buffer grid
        // Offset untuk Perlin Noise agar seed berpengaruh
        float noiseOffsetX = Random.Range(0f, 9999f);
        float noiseOffsetZ = Random.Range(0f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 1. TENTUKAN ZONA BERDASARKAN POSISI
                // Kita bagi peta menjadi 4 kuadran
                BiomeType biome = GetBiome(x, z, width, depth);

                // 2. HITUNG POSISI SUNGAI (Sinewave)
                // Sungai mengalir sepanjang sumbu X, bergelombang di sumbu Z
                float riverCenterZ = (depth / 2.0f) + Mathf.Sin(x / riverMeanderScale) * riverMeanderAmplitude;
                float distToRiver = Mathf.Abs(z - riverCenterZ);
                bool isRiver = distToRiver < riverWidth / 2.0f;

                // 3. TENTUKAN KETINGGIAN TANAH (Terrain Height)
                int ySurface = groundBaseHeight;

                if (biome == BiomeType.Plantation)
                {
                    // Perkebunan agak berbukit
                    float noise = Mathf.PerlinNoise((x * 0.1f) + noiseOffsetX, (z * 0.1f) + noiseOffsetZ);
                    ySurface += Mathf.RoundToInt(noise * 8); // Bukit setinggi max 8 blok
                }
                else if (isRiver)
                {
                    // Dasar sungai lebih dalam
                    ySurface = groundBaseHeight - 4;
                    if (ySurface < 1) ySurface = 1; // Jangan jebol ke void
                }

                // 4. GENERASI STRUKTUR (Gedung/Pabrik)
                int buildingH = 0;

                // Hanya bangun gedung jika bukan sungai
                if (!isRiver)
                {
                    if (biome == BiomeType.Urban)
                    {
                        // Urban: Gedung tinggi tapi kurus (Skyscrapers)
                        // Gunakan Noise diskrit untuk layout jalanan
                        if (ShouldBuild(x, z, 5, buildingDensity)) // Blok ukuran 5x5
                        {
                            buildingH = Random.Range(5, maxBuildingHeight);
                        }
                    }
                    else if (biome == BiomeType.Industrial)
                    {
                        // Industri: Gedung rendah tapi lebar (Pabrik)
                        if (ShouldBuild(x, z, 10, 0.6f)) // Blok besar 10x10
                        {
                            buildingH = Random.Range(3, 8);
                        }
                    }
                }

                // 5. ISI VOXEL VERTIKAL (Loop Y)
                for (int y = 0; y < height; y++)
                {
                    
                    VoxelCell cell = new VoxelCell();

                    // Logic Pengisian
                    if (y <= ySurface) // TANAH DASAR
                    {
                        cell.isSolid = true;
                        cell.amount = 0;
                        // Jika Perkebunan = Tipe 1 (Rumput), Jika Sungai/Urban = Tipe 3 (Tanah)
                        cell.blockType = (byte)(biome == BiomeType.Plantation ? 1 : 3);
                    }
                    else if (y <= ySurface + buildingH) // BANGUNAN
                    {
                        cell.isSolid = true;
                        cell.amount = 0;
                        // Urban = Tipe 2 (Beton), Industri = Tipe 4 (Bata)
                        cell.blockType = (byte)(biome == BiomeType.Urban ? 2 : 4);
                    }
                    else if (isRiver && y <= groundBaseHeight - 1) // AIR SUNGAI
                    {
                        // Isi air sampai ketinggian sedikit di bawah tanah normal
                        cell.isSolid = false;
                        cell.amount = 1.0f;
                        cell.blockType = 0; // Tipe air tidak butuh tekstur blok solid
                    }
                    else // UDARA
                    {
                        cell.isSolid = false;
                        cell.amount = 0;
                    }

                    voxelWorld.SetVoxel(x, y, z, cell);
                }
            }
        }

        Debug.Log("City Generated with Seed: " + seed);
    }

    // Helper untuk menentukan layout jalanan kota agar kotak-kotak rapi
    bool ShouldBuild(int x, int z, int blockSize, float density)
    {
        // Membuat grid jalanan
        if (x % blockSize == 0 || z % blockSize == 0) return false; // Ini Jalan Raya

        // Acak keberadaan gedung menggunakan hash sederhana agar deterministik
        // (Kita tidak pakai Random.value di sini agar konsisten per koordinat)
        float hash = Mathf.PerlinNoise(x * 0.5f + seed, z * 0.5f + seed);
        return hash < density;
    }

    BiomeType GetBiome(int x, int z, int w, int d)
    {
        // Bagi Peta Jadi 4 Kuadran (Tanpa Sungai)
        // Kiri Atas: Urban
        // Kanan Atas: Industri
        // Bawah: Perkebunan (Nature)

        int halfW = w / 2;
        int halfD = d / 2;

        if (z < halfD) // Bagian Bawah Peta
        {
            return BiomeType.Plantation;
        }
        else // Bagian Atas Peta
        {
            if (x < halfW) return BiomeType.Urban;
            else return BiomeType.Industrial;
        }
    }

    enum BiomeType { Urban, Industrial, Plantation, Water }
}