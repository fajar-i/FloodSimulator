using System.Collections.Generic;
using UnityEngine;

public class CityRendererSystem : MonoBehaviour
{
    private VoxelWorld world;
    public Material cityMaterial;
    private BuildingRegistry buildingDatabase;

    // Mapping ID -> Index Batch di List utama
    // Jika Connectable, int ini menunjuk ke index mesh pertama (Standalone)
    private Dictionary<byte, int> idToBatchIndex;

    // Penyimpanan Batch
    private List<List<Matrix4x4>> batches;
    private List<Mesh> batchMeshes;

    public void Initialize(VoxelWorld currentWorld, BuildingRegistry globalRegistry)
    {
        world = currentWorld;
        buildingDatabase = globalRegistry;
        InitializeBatches();
    }

    public void RebuildAllBatches()
    {
        if (world == null) return;
        InitializeBatches(); // Setup wadah batch berdasarkan Database terbaru

        Debug.Log("Renderer: Scanning World...");

        for (int x = 0; x < world.worldWidth; x++)
        {
            for (int z = 0; z < world.worldDepth; z++)
            {
                int y = FindSurfaceY(x, z);
                if (y == -1) continue;

                VoxelCell cell = world.GetVoxel(x, y, z);
                byte id = cell.blockType;

                // Cek apakah ID ini terdaftar di sistem batch kita?
                if (idToBatchIndex.ContainsKey(id))
                {
                    // Ambil Datanya
                    var data = buildingDatabase.GetDataByID(id);
                    int baseBatchIndex = idToBatchIndex[id];

                    if (data.renderType == BuildingRegistry.RenderType.StaticProp)
                    {
                        // --- LOGIKA STATIC (RUMAH/POHON) ---
                        Quaternion rot = Quaternion.Euler(0, cell.rotation * 90f, 0);
                        Vector3 pos = new Vector3(x + 0.5f, y, z + 0.5f) + data.visualOffset;
                        Vector3 scale = Vector3.one * (data.visualScale == 0 ? 1 : data.visualScale);

                        batches[baseBatchIndex].Add(Matrix4x4.TRS(pos, rot, scale));
                    }
                    else if (data.renderType == BuildingRegistry.RenderType.Connectable)
                    {
                        // --- LOGIKA CONNECTABLE (JALAN/SELOKAN) ---
                        // 1. Hitung Bitmask (Siapa tetangga saya?)
                        int mask = CalculateBitmask(world, x, y, z, id);

                        // 2. Terjemahkan Mask (0-15) menjadi Shape Index (0-5) & Rotasi
                        GetShapeAndRotation(mask, out int shapeIndex, out Quaternion rot);

                        // 3. Masukkan ke Batch yang tepat
                        // (baseBatchIndex + shapeIndex) karena kita alokasikan 6 slot berurutan
                        int targetBatch = baseBatchIndex + shapeIndex;

                        Vector3 pos = new Vector3(x + 0.5f, y + data.yOffset, z + 0.5f);

                        // Fix rotasi tegak (Blender Z-up vs Unity Y-up) jika perlu
                        // rot = rot * Quaternion.Euler(-90, 0, 0); // Aktifkan jika model tidur

                        batches[targetBatch].Add(Matrix4x4.TRS(pos, rot, Vector3.one));
                    }
                }
            }
        }
    }

    public void OnUpdate()
    {
        // Gambar semua batch yang sudah diisi
        for (int i = 0; i < batches.Count; i++)
        {
            if (batches[i].Count > 0)
                Graphics.DrawMeshInstanced(batchMeshes[i], 0, cityMaterial, batches[i]);
        }
    }

    // --- INISIALISASI PINTAR ---
    void InitializeBatches()
    {
        idToBatchIndex = new Dictionary<byte, int>();
        batches = new List<List<Matrix4x4>>();
        batchMeshes = new List<Mesh>();
        int counter = 0;

        foreach (var data in buildingDatabase.buildings)
        {
            if (data.renderType == BuildingRegistry.RenderType.StaticProp)
            {
                // Alokasi 1 Batch
                batches.Add(new List<Matrix4x4>());
                batchMeshes.Add(data.mesh);
                idToBatchIndex.Add(data.id, counter);
                counter++;
            }
            else if (data.renderType == BuildingRegistry.RenderType.Connectable)
            {
                // Alokasi 6 Batch Berurutan
                // Urutan: 0=Standalone, 1=End, 2=Straight, 3=Corner, 4=T, 5=Cross
                if (data.connectionMeshes == null || data.connectionMeshes.Length < 6)
                {
                    Debug.LogError($"Error: Item {data.name} (Connectable) tidak punya 6 mesh lengkap!");
                    continue;
                }

                idToBatchIndex.Add(data.id, counter); // Catat index awal

                for (int i = 0; i < 6; i++)
                {
                    batches.Add(new List<Matrix4x4>());
                    batchMeshes.Add(data.connectionMeshes[i]);
                    counter++;
                }
            }
        }
    }

    // --- LOGIKA MATEMATIKA BITMASK (UNIVERSAL) ---
    void GetShapeAndRotation(int mask, out int shapeIndex, out Quaternion rotation)
    {
        shapeIndex = 0; // Default Standalone
        rotation = Quaternion.identity;

        switch (mask)
        {
            case 0: shapeIndex = 0; break;

            case 1: shapeIndex = 1; rotation = Quaternion.Euler(0, 0, 0); break;   // N
            case 8: shapeIndex = 1; rotation = Quaternion.Euler(0, 180, 0); break; // S
            case 2: shapeIndex = 1; rotation = Quaternion.Euler(0, 270, 0); break; // W
            case 4: shapeIndex = 1; rotation = Quaternion.Euler(0, 90, 0); break;  // E

            case 9: shapeIndex = 2; rotation = Quaternion.Euler(0, 0, 0); break;   // Vert
            case 6: shapeIndex = 2; rotation = Quaternion.Euler(0, 90, 0); break;  // Horz

            case 3: shapeIndex = 3; rotation = Quaternion.Euler(0, -90, 0); break; // N+W
            case 5: shapeIndex = 3; rotation = Quaternion.Euler(0, 0, 0); break;   // N+E
            case 10: shapeIndex = 3; rotation = Quaternion.Euler(0, 180, 0); break;// S+W
            case 12: shapeIndex = 3; rotation = Quaternion.Euler(0, 90, 0); break; // S+E

            case 7: shapeIndex = 4; rotation = Quaternion.Euler(0, 0, 0); break;   // T (No S)
            case 11: shapeIndex = 4; rotation = Quaternion.Euler(0, -90, 0); break;// T (No E)
            case 13: shapeIndex = 4; rotation = Quaternion.Euler(0, 90, 0); break; // T (No W)
            case 14: shapeIndex = 4; rotation = Quaternion.Euler(0, 180, 0); break;// T (No N)

            case 15: shapeIndex = 5; break; // Cross
        }
    }

    int CalculateBitmask(VoxelWorld world, int x, int y, int z, byte myType)
    {
        int mask = 0;
        if (IsSameType(world, x, y, z + 1, myType)) mask += 1;
        if (IsSameType(world, x - 1, y, z, myType)) mask += 2;
        if (IsSameType(world, x + 1, y, z, myType)) mask += 4;
        if (IsSameType(world, x, y, z - 1, myType)) mask += 8;
        return mask;
    }

    bool IsSameType(VoxelWorld world, int x, int y, int z, byte type)
    {
        if (!world.IsValidIndex(x, y, z)) return false;
        // Opsional: Cek y+1 / y-1 untuk jalan menanjak
        return world.GetVoxel(x, y, z).blockType == type;
    }

    int FindSurfaceY(int x, int z)
    {
        for (int y = world.worldHeight - 1; y >= 0; y--)
            if (world.GetVoxel(x, y, z).isSolid) return y;
        return -1;
    }
}