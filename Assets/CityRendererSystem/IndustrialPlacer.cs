// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;

// public class IndustrialPlacer : MonoBehaviour
// {
//     [SerializeField] private VoxelWorld world;

//     // Struct untuk mendefinisikan Building
//     [System.Serializable]
//     public struct BuildingType
//     {
//         public string name;
//         public byte id;      // ID Block (misal 50)
//         public int width;    // Ukuran X (misal 2)
//         public int depth;    // Ukuran Z (misal 2)
//         public Mesh mesh;    // Model utuh (tidak dipotong)
//     }

//     public List<BuildingType> industrialBuildings; // Isi di Inspector (Tangki, Cerobong, dll)

//     // ID Zona Kuning (Industri)
//     private const byte ID_ZONE_INDUSTRIAL = 31;

//     public void GenerateIndustrialArea()
//     {
//         // 1. URUTKAN DARI YANG TERBESAR! (Sangat Penting)
//         // Kita mau pasang yang susah (besar) dulu, baru yang kecil.
//         var sortedBuildings = industrialBuildings
//             .OrderByDescending(b => b.width * b.depth)
//             .ToList();

//         // 2. Scan seluruh map
//         for (int x = 0; x < world.worldWidth; x++)
//         {
//             for (int z = 0; z < world.worldDepth; z++)
//             {
//                 // Kita hanya peduli pada pojok kiri bawah area
//                 // Cek apakah ini Zona Industri
//                 if (IsZone(x, z, ID_ZONE_INDUSTRIAL))
//                 {
//                     // 3. Coba pasang gedung satu per satu dari list
//                     foreach (var building in sortedBuildings)
//                     {
//                         // Cek apakah gedung ini muat di koordinat (x,z)
//                         if (CanPlaceBuildingCustom(x, z, building.width, building.depth))
//                         {
//                             PlaceBuilding(x, z, building);

//                             // JANGAN break/return. Lanjut ke koordinat sebelah 
//                             // karena mungkin gedung ini kecil dan sebelahnya masih muat
//                         }
//                     }
//                 }
//             }
//         }

//         world.UpdateAllChunks();
//     }

//     // Fungsi Cek Lahan (Modifikasi dari yang tadi)
//     bool CanPlaceBuildingCustom(int startX, int startZ, int width, int depth)
//     {
//         for (int x = startX; x < startX + width; x++) // Hapus buffer -1 agar rapat
//         {
//             for (int z = startZ; z < startZ + depth; z++)
//             {
//                 // Cek Out of Bounds
//                 if (!world.IsValidIndex(x, 0, z)) return false;

//                 int y = FindSurfaceY(x, z);
//                 if (y == -1) return false;

//                 VoxelCell cell = world.GetVoxel(x, y, z);

//                 // SYARAT UTAMA:
//                 // Lahannya HARUS Zona Industri (ID 31)
//                 // Dan BELUM ada bangunan (masih tanah datar/zona)
//                 if (cell.blockType != ID_ZONE_INDUSTRIAL)
//                 {
//                     return false; // Ada jalan, sungai, atau bangunan lain
//                 }
//             }
//         }
//         return true;
//     }

//     void PlaceBuilding(int startX, int startZ, BuildingType building)
//     {
//         // 1. Tentukan titik pusat rotasi (Pivot)
//         // Biasanya kita taruh data "Utama" di pojok kiri bawah (startX, startZ)
//         int y = FindSurfaceY(startX, startZ);

//         // Ubah Voxel di titik utama (Master Voxel)
//         VoxelCell masterCell = world.GetVoxel(startX, y, startZ);
//         masterCell.blockType = building.id; // ID Tangki
//         masterCell.rotation = 0; // Default rotasi
//         world.SetVoxelSilent(startX, y, startZ, masterCell);

//         // 2. TANDAI LAHAN SISA SEBAGAI "TERPAKAI" (Slave Voxels)
//         // Agar tidak ditimpa bangunan lain nanti.
//         // Kita pakai ID dummy, misal 255 (Reserved)
//         for (int x = startX; x < startX + building.width; x++)
//         {
//             for (int z = startZ; z < startZ + building.depth; z++)
//             {
//                 // Jangan timpa Master Voxel
//                 if (x == startX && z == startZ) continue;

//                 int surfY = FindSurfaceY(x, z);
//                 VoxelCell filler = world.GetVoxel(x, surfY, z);
//                 filler.blockType = 255; // "Jangan Ganggu"
//                 world.SetVoxelSilent(x, surfY, z, filler);
//             }
//         }
//     }

//     bool IsZone(int x, int z, byte zoneID)
//     {
//         int y = FindSurfaceY(x, z);
//         if (y == -1) return false;
//         return world.GetVoxel(x, y, z).blockType == zoneID;
//     }

//     int FindSurfaceY(int x, int z)
//     {
//         for (int y = world.worldHeight - 1; y >= 0; y--)
//             if (world.GetVoxel(x, y, z).isSolid) return y;
//         return -1;
//     }
// }