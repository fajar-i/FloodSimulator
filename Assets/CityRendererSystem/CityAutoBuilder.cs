using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CityAutoBuilder : MonoBehaviour
{
    private VoxelWorld world;
    private BuildingRegistry registry;

    // Batas toleransi kemiringan. 
    // Jika beda tinggi tanah > 2 blok, dianggap tebing curam & batal bangun.
    private const int MAX_SLOPE_TOLERANCE = 3; 

    public void Initialize(VoxelWorld _world, BuildingRegistry _reg)
    {
        this.world = _world;
        this.registry = _reg;
    }

    public void BuildEntireCity()
    {
        if (world == null) return;
        Debug.Log("AutoBuilder: Starting Smart Construction on Perlin Terrain...");

        // Ambil data dari Registry
        var industries = registry.buildings
                            .Where(b => b.zone == ZoneType.INDUSTRIAL)
                            .OrderByDescending(b => b.width * b.depth).ToList();
        
        var houses = registry.buildings
                            .Where(b => b.zone == ZoneType.RESIDENTIAL)
                            .OrderByDescending(b => b.width * b.depth).ToList();

        // Pass 1: Industri (Butuh lahan rata yang luas)
        PlaceSmartBuildings(industries, VoxelID.ZONE_INDUSTRIAL); // ID Zone Industrial

        // Pass 2: Perumahan
        PlaceSmartBuildings(houses, VoxelID.ZONE_RESIDENTIAL); // ID Zone Residential

        // Update visual chunk di akhir (Manager yang panggil, tapi kalau mau tes bisa di sini)
        world.UpdateAllChunks();
    }

    void PlaceSmartBuildings(List<BuildingRegistry.BuildingData> buildings, byte targetZoneID)
    {
        // Scan seluruh map
        for (int x = 0; x < world.worldWidth; x++)
        {
            for (int z = 0; z < world.worldDepth; z++)
            {
                // Cek apakah koordinat ini adalah kandidat zona yang tepat
                // Kita pakai FindSurfaceY untuk dapat Y tanah tertinggi di titik ini
                int currentY = FindSurfaceY(x, z);
                if (currentY == -1) continue;

                VoxelCell currentCell = world.GetVoxel(x, currentY, z);
                if (currentCell.blockType != targetZoneID) continue;

                // Coba pasang bangunan
                foreach (var b in buildings)
                {
                    // 1. CARI JALAN & TENTUKAN TARGET KETINGGIAN (CRUCIAL!)
                    // Kita butuh tahu Y jalan agar rumah sejajar dengan jalan.
                    if (TryGetRoadInfo(x, z, b.width, b.depth, out int roadY, out int rotation))
                    {
                        // 2. CEK KELAYAKAN TERRAIN (Apakah terlalu curam?)
                        if (IsTerrainBuildable(x, z, b.width, b.depth, roadY))
                        {
                            // 3. RATAKAN TANAH & BANGUN
                            FlattenAndBuild(x, z, roadY, b, rotation);
                            
                            // Loncat ke tile berikutnya agar tidak tumpang tindih
                            goto NextTile;
                        }
                    }
                }
                NextTile:;
            }
        }
    }

    // --- LOGIKA CERDAS BARU ---

    // Mengembalikan TRUE jika ada jalan menempel, sekaligus memberi tahu Y jalan dan Rotasi
    bool TryGetRoadInfo(int x, int z, int w, int d, out int roadY, out int rotation)
    {
        roadY = -1;
        rotation = -1;

        // Cek 4 Sisi untuk mencari jalan (ID 10)
        // Kita juga mengambil Y dari jalan tersebut.
        
        // Utara
        if (CheckLineForRoad(x, z + d, x + w, z + d, true, out roadY)) { rotation = 0; return true; }
        // Timur
        if (CheckLineForRoad(x + w, z, x + w, z + d, false, out roadY)) { rotation = 1; return true; }
        // Selatan
        if (CheckLineForRoad(x, z - 1, x + w, z - 1, true, out roadY)) { rotation = 2; return true; }
        // Barat
        if (CheckLineForRoad(x - 1, z, x - 1, z + d, false, out roadY)) { rotation = 3; return true; }

        return false;
    }

    bool CheckLineForRoad(int x1, int z1, int x2, int z2, bool horizontal, out int foundY)
    {
        foundY = -1;
        // Loop sepanjang garis sisi
        int steps = horizontal ? (x2 - x1) : (z2 - z1);
        
        for (int i = 0; i < steps; i++)
        {
            int cx = horizontal ? x1 + i : x1;
            int cz = horizontal ? z1 : z1 + i;

            // Cari permukaan tanah di titik tersebut
            int y = FindSurfaceY(cx, cz);
            if (y == -1) continue;

            // Asumsi ID Jalan
            if (world.GetVoxel(cx, y, cz).blockType == VoxelID.ROAD)
            {
                foundY = y; // KETEMU! Ini ketinggian jalan.
                return true;
            }
        }
        return false;
    }

    // Cek apakah tanah terlalu curam untuk diratakan
    bool IsTerrainBuildable(int startX, int startZ, int w, int d, int targetY)
    {
        int minH = 999;
        int maxH = -999;

        for (int x = startX; x < startX + w; x++)
        {
            for (int z = startZ; z < startZ + d; z++)
            {
                if (!world.IsValidIndex(x, 0, z)) return false;
                
                int y = FindSurfaceY(x, z);
                if (y == -1 ) return false; // Jurang

                // Cek apakah area ini sudah ada bangunan lain?
                // Kita hanya boleh meratakan Zona atau Tanah
                byte type = world.GetVoxel(x, y, z).blockType;
                if (type != VoxelID.ZONE_RESIDENTIAL && type != VoxelID.ZONE_INDUSTRIAL && type != VoxelID.GRASS) return false; 

                if (y < minH) minH = y;
                if (y > maxH) maxH = y;
            }
        }

        // Kalau beda tinggi tanah asli vs Target Jalan terlalu jauh, batalkan.
        // Contoh: Jalan di Y=10, tapi tanah di Y=5 (Jurang) atau Y=15 (Tebing)
        if (Mathf.Abs(maxH - targetY) > MAX_SLOPE_TOLERANCE) return false;
        if (Mathf.Abs(minH - targetY) > MAX_SLOPE_TOLERANCE) return false;

        return true;
    }

    void FlattenAndBuild(int startX, int startZ, int floorY, BuildingRegistry.BuildingData b, int rotation)
    {
        // 1. TERRAFORMING LOOP
        for (int x = startX; x < startX + b.width; x++)
        {
            for (int z = startZ; z < startZ + b.depth; z++)
            {
                int currentY = FindSurfaceY(x, z);

                // A. CUT (Potong Bukit)
                // Jika tanah asli lebih tinggi dari lantai bangunan, hapus kelebihannya
                if (currentY > floorY)
                {
                    for (int k = currentY; k > floorY; k--)
                    {
                        VoxelCell air = new VoxelCell { blockType = VoxelID.WATER, isSolid = false }; // Air = 0
                        world.SetVoxelSilent(x, k, z, air);
                    }
                }

                // B. FILL (Timbun Lembah & Pondasi)
                // Pastikan blok tepat di lantai (floorY) itu padat
                // Dan pastikan di bawahnya tidak bolong (Foundation)
                for (int k = floorY; k >= 0; k--)
                {
                    VoxelCell existing = world.GetVoxel(x, k, z);
                    
                    // Jika ketemu blok solid di bawah, stop (pondasi sudah menyentuh tanah keras)
                    if (existing.isSolid && k < floorY) break;

                    // Isi dengan Beton/Tanah agar rumah tidak melayang
                    VoxelCell foundation = existing;
                    foundation.blockType = VoxelID.GRASS; // Pondasi Tanah
                    foundation.isSolid = true;
                    world.SetVoxelSilent(x, k, z, foundation);
                }

                // C. TANDAI AREA SEBAGAI "TERPAKAI" (Filler)
                // Set blok di floorY menjadi ID Filler agar tidak ditimpa bangunan lain
                VoxelCell filler = world.GetVoxel(x, floorY, z);
                filler.blockType = VoxelID.GRASS; 
                world.SetVoxelSilent(x, floorY, z, filler);
            }
        }

        // 2. PLACE MASTER VOXEL (Gedung Asli)
        // Taruh tepat di ketinggian floorY (sejajar jalan)
        VoxelCell master = world.GetVoxel(startX, floorY, startZ);
        master.blockType = b.id;
        master.rotation = (byte)rotation;
        world.SetVoxelSilent(startX, floorY, startZ, master);
    }

    int FindSurfaceY(int x, int z)
    {
        // Scan dari atas ke bawah, cari blok solid pertama
        for (int y = world.worldHeight - 1; y >= 0; y--)
        {
            if (world.GetVoxel(x, y, z).isSolid) return y;
        }
        return -1; // Void
    }
}