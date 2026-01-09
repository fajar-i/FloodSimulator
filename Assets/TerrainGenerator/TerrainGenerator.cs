using UnityEngine;
using Unity.Collections;

public class TerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public int seed = 12345;
    public int groundBaseHeight = 5;

    [Header("River Settings")]
    public int riverWidth = 6;
    public float riverMeanderScale = 30f;
    public float riverMeanderAmplitude = 30f;

    [Header("Noise Settings")]
    public float terrainScale = 0.05f; // Seberapa "zoom" noise-nya
    public int terrainHeightMultiplier = 12; // Tinggi maksimal bukit

    // Method publik yang dipanggil oleh GameManager
    public void GenerateTerrain(VoxelWorld voxelWorld)
    {
        Random.InitState(seed);
        GenerateTerrainData(voxelWorld);
        voxelWorld.UpdateAllChunks();
    }

    void GenerateTerrainData(VoxelWorld voxelWorld)
    {
        int width = voxelWorld.worldWidth;
        int height = voxelWorld.worldHeight;
        int depth = voxelWorld.worldDepth;

        float noiseOffsetX = Random.Range(0f, 9999f);
        float noiseOffsetZ = Random.Range(0f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 1. TENTUKAN BIOME (Untuk warna tanah: Rumput/Beton/Pasir)
                BiomeType biome = GetBiome(x, z, width, depth);

                // 2. HITUNG POSISI SUNGAI
                float riverCenterZ = (depth / 2.0f) + Mathf.Sin(x / riverMeanderScale) * riverMeanderAmplitude;
                float distToRiver = Mathf.Abs(z - riverCenterZ);
                bool isRiver = distToRiver < riverWidth / 2.0f;

                // 3. TENTUKAN KETINGGIAN TANAH (Terrain Height)
                int ySurface = groundBaseHeight;

                if (biome == BiomeType.Plantation)
                {
                    // Hanya area Perkebunan yang berbukit
                    float noise = Mathf.PerlinNoise((x * terrainScale) + noiseOffsetX, (z * terrainScale) + noiseOffsetZ);
                    ySurface += Mathf.RoundToInt(noise * terrainHeightMultiplier);
                }

                if (isRiver)
                {
                    // Gali tanah untuk sungai
                    ySurface = groundBaseHeight - 4;
                    if (ySurface < 1) ySurface = 1;
                }
                int maxFillHeight = Mathf.Max(ySurface, groundBaseHeight);
                // 4. ISI VOXEL (Hanya Tanah & Air)
                for (int y = 0; y < maxFillHeight; y++)
                {
                    VoxelCell cell = new VoxelCell();

                    if (y <= ySurface) // TANAH PADAT
                    {
                        cell.isSolid = true;
                        cell.amount = 0;

                        // Tentukan Texture berdasarkan Biome
                        if (biome == BiomeType.Plantation) cell.blockType = 2; // Rumput
                        else if (biome == BiomeType.Urban) cell.blockType = 2; // Beton/Aspal
                        else if (biome == BiomeType.Industrial) cell.blockType = 3; // Tanah Kasar/Lumpur
                    }
                    else if (isRiver && y <= groundBaseHeight - 1) // AIR
                    {
                        cell.isSolid = false;
                        cell.amount = 1.0f;
                        cell.blockType = 0; // Air
                    }
                    else // UDARA
                    {
                        cell.isSolid = false;
                        cell.amount = 0;
                    }

                    voxelWorld.SetVoxelSilent(x, y, z, cell);
                }
            }
        }
        Debug.Log("[TerrainGenerator] Terrain Generated.");
    }

    BiomeType GetBiome(int x, int z, int w, int d)
    {
        // int halfW = w / 2;
        // int halfD = d / 2;
        // if (z < halfD) return BiomeType.Plantation;
        // else return (x < halfW) ? BiomeType.Urban : BiomeType.Industrial;
        return BiomeType.Plantation;
    }

    enum BiomeType { Urban, Industrial, Plantation }
}