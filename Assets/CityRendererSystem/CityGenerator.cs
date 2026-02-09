using UnityEngine;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour
{
    private VoxelWorld world;

    [Header("Settings")]
    public int mainRoadInterval = 32; // Jarak antar jalan utama (Arteri)
    public int localRoadInterval = 10; // Jarak antar jalan gang (Lokal)
    public int cityPadding = 5; // Jarak aman dari pinggir peta

    // ID BLOK
    private const byte ID_ROAD = 10;
    private const byte ID_BRIDGE = 12; // Jika kena sungai
    private const byte ID_ZONE_RESIDENTIAL = 30;
    private const byte ID_RIVER = 8; // ID Sungai alami (asumsi)
    public void GenerateCityLayout(VoxelWorld connectedWorld)
    {
        world = connectedWorld;
        // "Hierarchical Grid Partitioning" (Pembagian Grid Bertingkat).
        Debug.Log("Generating Road Network...");

        // 1. Generate Jalan Vertikal (Sumbu Z)
        for (int x = cityPadding; x < world.worldWidth - cityPadding; x++)
        {
            // Cek apakah ini koordinat untuk Jalan Utama ATAU Jalan Lokal
            bool isMainRoad = (x % mainRoadInterval == 0);
            bool isLocalRoad = (x % localRoadInterval == 0);

            if (isMainRoad || isLocalRoad)
            {
                // Tarik garis jalan dari bawah ke atas map
                CreateRoadLine(x, 0, 0, 1, world.worldDepth);
            }
        }

        // 2. Generate Jalan Horizontal (Sumbu X)
        for (int z = cityPadding; z < world.worldDepth - cityPadding; z++)
        {
            bool isMainRoad = (z % mainRoadInterval == 0);
            bool isLocalRoad = (z % localRoadInterval == 0);

            if (isMainRoad || isLocalRoad)
            {
                // Tarik garis jalan dari kiri ke kanan map
                CreateRoadLine(0, z, 1, 0, world.worldWidth);
            }
        }

        // 3. Isi Sela-sela dengan Zoning (WFC Preparation)
        FillZones();
        // // 4. Update Visual
        // world.UpdateAllChunks();
    }

    // Fungsi Pembantu: Menarik Garis Jalan
    // dirX/dirZ menentukan arah (1,0 = Horizontal, 0,1 = Vertikal)
    void CreateRoadLine(int startX, int startZ, int dirX, int dirZ, int length)
    {
        for (int i = 0; i < length; i++)
        {
            int x = startX + (i * dirX);
            int z = startZ + (i * dirZ);

            // Cek Boundary
            if (x < cityPadding || x >= world.worldWidth - cityPadding ||
                z < cityPadding || z >= world.worldDepth - cityPadding)
                continue;

            // Cari ketinggian tanah
            int y = FindSurfaceY(x, z);
            if (y == -1) continue; // Jurang

            VoxelCell cell = world.GetVoxel(x, y, z);

            // --- LOGIKA JEMBATAN VS JALAN ---
            if (cell.blockType == ID_RIVER) // Jika menabrak sungai
            {
                cell.blockType = ID_BRIDGE;
                // Opsional: Naikkan y+1 agar jembatan di atas air
            }
            else
            {
                cell.blockType = ID_ROAD;
            }

            // Simpan perubahan
            world.SetVoxelSilent(x, y, z, cell);

            // OPTIONAL: Ratakan tanah di sekitar jalan (agar tidak naik turun drastis)
            // FlattenTerrain(x, y, z); 
        }
    }

    // Mengisi area kosong di antara jalan dengan Zona (Warna lantai)
    void FillZones()
    {
        for (int x = 0; x < world.worldWidth; x++)
        {
            for (int z = 0; z < world.worldDepth; z++)
            {
                int y = FindSurfaceY(x, z);
                if (y == -1) continue;

                VoxelCell cell = world.GetVoxel(x, y, z);

                // Jika TANAH BIASA (ID 1) dan bukan Jalan/Sungai/Jembatan
                // Ubah menjadi Zona Perumahan (30)
                if (cell.blockType == 1)
                {
                    cell.blockType = ID_ZONE_RESIDENTIAL;
                    world.SetVoxelSilent(x, y, z, cell);
                }
            }
        }
    }

    int FindSurfaceY(int x, int z)
    {
        for (int y = world.worldHeight - 1; y >= 0; y--)
            if (world.GetVoxel(x, y, z).isSolid) return y;
        return -1;
    }
}