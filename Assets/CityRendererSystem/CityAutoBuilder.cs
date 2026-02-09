using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CityAutoBuilder : MonoBehaviour
{

    [Header("Building Database")]
    private VoxelWorld world;
    private BuildingRegistry buildingDatabase;

    [Header("Settings")]
    public byte roadID = 10;
    public byte residentialZoneID = 30;
    public byte industrialZoneID = 31;
    public byte treeID = 60; // ID baru untuk Pohon
    public void Initialize(VoxelWorld _world, BuildingRegistry _registry)
    {
        this.world = _world;
        this.buildingDatabase = _registry;
    }
    // Fungsi Utama yang dipanggil Manager
    public void BuildEntireCity()
    {
        Debug.Log("Building City...");

        // 1. PISAHKAN LIST BANGUNAN
        var industries = buildingDatabase.buildings
                                    .Where(b => b.zone == ZoneType.Industrial)
                                    .OrderByDescending(b => b.width * b.depth).ToList();

        var houses = buildingDatabase.buildings
                            .Where(b => b.zone == ZoneType.Residential)
                            .OrderByDescending(b => b.width * b.depth).ToList();        // 2. PASS 1: BANGUN INDUSTRI (Prioritas Besar)
        PlaceZoneBuildings(industries, industrialZoneID);

        // 3. PASS 2: BANGUN PERUMAHAN
        PlaceZoneBuildings(houses, residentialZoneID);

        // 4. PASS 3: DEKORASI (Isi celah kosong)
        FillDecorations();

        // 5. UPDATE VISUAL
        // world.UpdateAllChunks();
    }

    void PlaceZoneBuildings(List<BuildingRegistry.BuildingData> buildings, byte targetZoneID)
    {
        for (int x = 0; x < world.worldWidth; x++)
        {
            for (int z = 0; z < world.worldDepth; z++)
            {
                // Cek apakah tanah ini adalah Zona yang tepat
                if (!IsZone(x, z, targetZoneID)) continue;

                // Coba pasang bangunan satu per satu dari list (Besar -> Kecil)
                foreach (var b in buildings)
                {
                    // Cek Ukuran & Cek Arah Jalan
                    int rotation = GetRotationToRoad(x, z, b.width, b.depth);

                    // Jika rotation != -1 artinya ADA JALAN di dekatnya & Muat
                    if (rotation != -1)
                    {
                        // Cek apakah area benar-benar kosong (Double check)
                        if (CanPlace(x, z, b.width, b.depth, targetZoneID))
                        {
                            PlaceVoxel(x, z, b, rotation);
                            // Lanjut ke koordinat grid berikutnya, jangan tumpuk gedung di sini
                            goto NextTile;
                        }
                    }
                }
            NextTile:;
            }
        }
    }

    // --- LOGIKA ROTASI (CRUCIAL) ---
    // Mengembalikan 0, 1, 2, 3 jika ada jalan. Mengembalikan -1 jika tidak ada jalan.
    int GetRotationToRoad(int x, int z, int w, int d)
    {
        // Kita cek 4 sisi kotak bangunan.
        // Asumsi: Pivot bangunan ada di pojok kiri bawah (0,0) lokal.
        // Rotation 0 = Depan menghadap Utara (Z+)
        // Rotation 1 = Depan menghadap Timur (X+)
        // Rotation 2 = Depan menghadap Selatan (Z-)
        // Rotation 3 = Depan menghadap Barat (X-)

        // Cek UTARA (Z + d)
        if (CheckLineForRoad(x, z + d, x + w, z + d, true)) return 0;

        // Cek TIMUR (X + w)
        if (CheckLineForRoad(x + w, z, x + w, z + d, false)) return 1;

        // Cek SELATAN (Z - 1)
        if (CheckLineForRoad(x, z - 1, x + w, z - 1, true)) return 2;

        // Cek BARAT (X - 1)
        if (CheckLineForRoad(x - 1, z, x - 1, z + d, false)) return 3;

        return -1; // Tidak ada jalan menempel
    }

    bool CheckLineForRoad(int x1, int z1, int x2, int z2, bool horizontal)
    {
        // Loop sepanjang garis sisi bangunan untuk cari jalan
        if (horizontal) // Loop X
        {
            for (int i = x1; i < x2; i++)
                if (IsRoad(i, z1)) return true;
        }
        else // Loop Z
        {
            for (int i = z1; i < z2; i++)
                if (IsRoad(x1, i)) return true;
        }
        return false;
    }

    // --- LOGIKA PENEMPATAN ---
    void PlaceVoxel(int startX, int startZ, BuildingRegistry.BuildingData b, int rotation)
    {
        int y = FindSurfaceY(startX, startZ);

        // 1. Master Voxel (Pivot)
        VoxelCell master = world.GetVoxel(startX, y, startZ);
        master.blockType = b.id;
        master.rotation = (byte)rotation;
        world.SetVoxelSilent(startX, y, startZ, master);

        // 2. Filler Voxels (Agar tidak ditimpa)
        for (int x = startX; x < startX + b.width; x++)
        {
            for (int z = startZ; z < startZ + b.depth; z++)
            {
                if (x == startX && z == startZ) continue; // Skip master

                int fy = FindSurfaceY(x, z);
                if (fy != -1)
                {
                    VoxelCell filler = world.GetVoxel(x, fy, z);
                    filler.blockType = 255; // ID "Terisi"
                    world.SetVoxelSilent(x, fy, z, filler);
                }
            }
        }
    }

    void FillDecorations()
    {
        // Scan ulang seluruh map untuk sisa zona kosong
        for (int x = 0; x < world.worldWidth; x++)
        {
            for (int z = 0; z < world.worldDepth; z++)
            {
                int y = FindSurfaceY(x, z);
                if (y == -1) continue;

                VoxelCell cell = world.GetVoxel(x, y, z);

                // Jika masih berupa ZONA (belum jadi gedung/terisi)
                if (cell.blockType == residentialZoneID || cell.blockType == industrialZoneID)
                {
                    // 30% kemungkinan muncul pohon
                    if (Random.value < 0.3f)
                    {
                        cell.blockType = treeID;
                        cell.rotation = (byte)Random.Range(0, 4); // Rotasi acak biar natural
                        world.SetVoxelSilent(x, y, z, cell);
                    }
                    else
                    {
                        // Sisanya kembalikan jadi tanah biasa atau biarkan zona (taman)
                        // cell.blockType = 1; 
                    }
                }
            }
        }
    }

    // --- HELPERS ---
    bool CanPlace(int startX, int startZ, int w, int d, byte zoneID)
    {
        for (int x = startX; x < startX + w; x++)
        {
            for (int z = startZ; z < startZ + d; z++)
            {
                if (!world.IsValidIndex(x, 0, z)) return false;
                int y = FindSurfaceY(x, z);
                if (y == -1) return false;

                // Pastikan lahannya adalah Zona yang diminta
                if (world.GetVoxel(x, y, z).blockType != zoneID) return false;
            }
        }
        return true;
    }

    bool IsZone(int x, int z, byte id)
    {
        int y = FindSurfaceY(x, z);
        return y != -1 && world.GetVoxel(x, y, z).blockType == id;
    }

    bool IsRoad(int x, int z)
    {
        int y = FindSurfaceY(x, z);
        return y != -1 && world.GetVoxel(x, y, z).blockType == roadID;
    }

    int FindSurfaceY(int x, int z)
    {
        for (int y = world.worldHeight - 1; y >= 0; y--)
            if (world.GetVoxel(x, y, z).isSolid) return y;
        return -1;
    }
}