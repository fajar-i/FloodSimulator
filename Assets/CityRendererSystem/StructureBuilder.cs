// using System.Collections.Generic;
// using UnityEngine;

// public class StructureBuilder : MonoBehaviour
// {
//     [SerializeField] private VoxelWorld world;

//     // --- ID REFERENSI ---
//     private const byte ID_ROAD = 10;
//     private const byte ID_ZONE_RESIDENTIAL = 30; // Zona Hijau
//     private const byte ID_ZONE_INDUSTRIAL = 31;  // Zona Kuning
    
//     // Hasil WFC (Gedung Jadi)
//     private const byte ID_HOUSE_SMALL = 40;
//     private const byte ID_FACTORY_SMALL = 41;

//     // --- FUNGSI UTAMA WFC ---
//     public void RunWFC()
//     {
//         Debug.Log("Memulai Konstruksi Kota...");
        
//         // 1. Kumpulkan semua kandidat (Area yang sudah di-zoning player)
//         // Kita pakai List agar bisa di-acak (Shuffle) biar variatif
//         List<Vector3Int> candidates = new List<Vector3Int>();

//         for (int x = 0; x < world.worldWidth; x++)
//         {
//             for (int z = 0; z < world.worldDepth; z++)
//             {
//                 int y = FindSurfaceY(x, z);
//                 if (y == -1) continue;

//                 VoxelCell cell = world.GetVoxel(x, y, z);
                
//                 // Jika ini adalah ZONA (30 atau 31), masukkan ke daftar
//                 if (cell.blockType == ID_ZONE_RESIDENTIAL || cell.blockType == ID_ZONE_INDUSTRIAL)
//                 {
//                     candidates.Add(new Vector3Int(x, y, z));
//                 }
//             }
//         }

//         // 2. Acak urutan (Biar kota terlihat organik, tidak dibangun urut dari pojok)
//         Shuffle(candidates);

//         // 3. Proses Collapse (Ubah Zona jadi Gedung)
//         foreach (var pos in candidates)
//         {
//             CollapseCell(pos.x, pos.y, pos.z);
//         }

//         // 4. Update Visual Sekaligus
//         world.UpdateAllChunks();
//     }

//     void CollapseCell(int x, int y, int z)
//     {
//         VoxelCell cell = world.GetVoxel(x, y, z);
        
//         // Cek Tetangga: Di mana jalannya?
//         // Kita butuh arah jalan untuk menentukan rotasi gedung
//         int roadDirection = FindRoadNeighbor(x, y, z);

//         // Jika TIDAK ADA jalan di sebelah, batalkan pembangunan (tetap jadi tanah kosong)
//         if (roadDirection == -1) return; 

//         // Tentukan Gedung berdasarkan Zona
//         // TODO : randomize gedung
//         byte buildingID = 0;
//         if (cell.blockType == ID_ZONE_RESIDENTIAL) buildingID = ID_HOUSE_SMALL;
//         else if (cell.blockType == ID_ZONE_INDUSTRIAL) buildingID = ID_FACTORY_SMALL;

//         // Terapkan Perubahan
//         cell.blockType = buildingID;
//         cell.rotation = (byte)roadDirection; // Simpan rotasi (0, 1, 2, 3)
        
//         // Simpan ke World (Pakai Silent mode biar cepat, nanti update di akhir)
//         world.SetVoxelSilent(x, y, z, cell);
//     }

//     // Mengembalikan arah jalan: 0=Utara, 1=Timur, 2=Selatan, 3=Barat. -1=Gak ada jalan.
//     int FindRoadNeighbor(int x, int y, int z)
//     {
//         // Prioritas Arah (Bisa diacak juga kalau mau lebih variatif)
//         // Cek UTARA (Z+1)
//         if (IsRoad(x, y, z + 1)) return 0; // Gedung menghadap Utara (0 derajat)
//         // Cek TIMUR (X+1)
//         if (IsRoad(x + 1, y, z)) return 1; // Gedung menghadap Timur (90 derajat)
//         // Cek SELATAN (Z-1)
//         if (IsRoad(x, y, z - 1)) return 2; // Gedung menghadap Selatan (180 derajat)
//         // Cek BARAT (X-1)
//         if (IsRoad(x - 1, y, z)) return 3; // Gedung menghadap Barat (270 derajat)

//         return -1; // Terisolasi, tidak ada akses jalan
//     }

//     bool IsRoad(int x, int y, int z)
//     {
//         if (!world.IsValidIndex(x, y, z)) return false;
//         // Asumsi jalan ada di ketinggian yang sama. 
//         // Kalau jalan bisa naik turun, cek y-1 atau y+1 juga.
//         return world.GetVoxel(x, y, z).blockType == ID_ROAD;
//     }

//     // Helper: Fisher-Yates Shuffle
//     void Shuffle<T>(List<T> list)
//     {
//         for (int i = 0; i < list.Count; i++)
//         {
//             int rnd = Random.Range(i, list.Count);
//             (list[rnd], list[i]) = (list[i], list[rnd]);
//         }
//     }
    
//     int FindSurfaceY(int x, int z)
//     {
//         for (int y = world.worldHeight - 1; y >= 0; y--)
//             if (world.GetVoxel(x, y, z).isSolid) return y;
//         return -1;
//     }
// }