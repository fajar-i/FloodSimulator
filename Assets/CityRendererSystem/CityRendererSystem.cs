// using System.Collections.Generic;
// using UnityEngine;

// public class CityRendererSystem : MonoBehaviour
// {
//     public VoxelWorld world;
//     public Material cityMaterial; // Material Atlas

//     [Header("Gutter Meshes")]
//     public Mesh meshStandalone;
//     public Mesh meshEnd;
//     public Mesh meshStraight;
//     public Mesh meshCorner;
//     public Mesh meshT;
//     public Mesh meshCross;

//     // ID Selokan yang akan dirender (Sesuai dengan Controller tadi)
//     private const byte ID_GUTTER = 11;

//     // Buffer untuk GPU Instancing
//     private List<Matrix4x4>[] batchLists; // Array of Lists (untuk tiap mesh)
//     private Mesh[] meshTypes; // Array referensi mesh

//     void Start()
//     {
//         // Setup sederhana untuk menampung batch data
//         // Urutan: 0=Standalone, 1=End, 2=Straight, 3=Corner, 4=T, 5=Cross
//         batchLists = new List<Matrix4x4>[6];
//         for (int i = 0; i < 6; i++) batchLists[i] = new List<Matrix4x4>();

//         meshTypes = new Mesh[] { meshStandalone, meshEnd, meshStraight, meshCorner, meshT, meshCross };
//     }

//     void Update()
//     {
//         if (world == null) return;

//         // 1. BERSIHKAN DATA LAMA
//         for (int i = 0; i < 6; i++) batchLists[i].Clear();

//         // 2. SCANNING & BITMASKING
//         // (Optimasi: Nanti bisa dipindah agar tidak loop setiap frame, tapi untuk sekarang oke)
//         for (int x = 0; x < world.worldWidth; x++)
//         {
//             for (int z = 0; z < world.worldDepth; z++)
//             {
//                 // Kita cari permukaan tanah lagi untuk render
//                 // (Atau simpan koordinat ini di data lain agar cepat)
//                 int y = FindSurfaceY(x, z); 
//                 if (y == -1) continue;

//                 VoxelCell cell = world.GetVoxel(x, y, z);

//                 // Jika ketemu blok Selokan (ID 11)
//                 if (cell.blockType == ID_GUTTER)
//                 {
//                     int mask = CalculateBitmask(world, x, y, z, ID_GUTTER);
//                     AddToBatch(x, y, z, mask);
//                 }
//             }
//         }

//         // 3. GAMBAR KE LAYAR (GPU INSTANCING)
//         for (int i = 0; i < 6; i++)
//         {
//             if (batchLists[i].Count > 0 && meshTypes[i] != null)
//             {
//                 Graphics.DrawMeshInstanced(
//                     meshTypes[i], 
//                     0, 
//                     cityMaterial, 
//                     batchLists[i]
//                 );
//             }
//         }
//     }

//     // --- LOGIKA BITMASKING (Pindahan dari kode Anda) ---
//     int CalculateBitmask(VoxelWorld world, int x, int y, int z, byte myType)
//     {
//         int mask = 0;
//         if (IsConnectable(world, x, y, z + 1, myType)) mask += 1; // Utara
//         if (IsConnectable(world, x - 1, y, z, myType)) mask += 2; // Barat
//         if (IsConnectable(world, x + 1, y, z, myType)) mask += 4; // Timur
//         if (IsConnectable(world, x, y, z - 1, myType)) mask += 8; // Selatan
//         return mask;
//     }

//     bool IsConnectable(VoxelWorld world, int x, int y, int z, byte myType)
//     {
//         // Cek batas array dulu
//         if (x < 0 || x >= world.worldWidth || z < 0 || z >= world.worldDepth) return false;
        
//         // Asumsi selokan ada di ketinggian yang sama (y)
//         // Kalau mau lebih canggih, cek y+1 atau y-1 juga (tanjakan)
//         VoxelCell neighbor = world.GetVoxel(x, y, z);
//         return neighbor.blockType == myType;
//     }

//     void AddToBatch(int x, int y, int z, int mask)
//     {
//         int meshIndex = 0;
//         Quaternion rotation = Quaternion.identity;

//         // Logika Switch Case Anda yang Tepat
//         switch (mask)
//         {
//             case 0: meshIndex = 0; break; // Standalone

//             case 1: meshIndex = 1; rotation = Quaternion.Euler(0, 0, 0); break;   
//             case 8: meshIndex = 1; rotation = Quaternion.Euler(0, 180, 0); break; 
//             case 2: meshIndex = 1; rotation = Quaternion.Euler(0, 270, 0); break; 
//             case 4: meshIndex = 1; rotation = Quaternion.Euler(0, 90, 0); break;  

//             case 9: meshIndex = 2; rotation = Quaternion.Euler(0, 0, 0); break;  // Straight Vert
//             case 6: meshIndex = 2; rotation = Quaternion.Euler(0, 90, 0); break; // Straight Horz

//             case 3: meshIndex = 3; rotation = Quaternion.Euler(0, -90, 0); break;
//             case 5: meshIndex = 3; rotation = Quaternion.Euler(0, 0, 0); break;
//             case 10: meshIndex = 3; rotation = Quaternion.Euler(0, 180, 0); break;
//             case 12: meshIndex = 3; rotation = Quaternion.Euler(0, 90, 0); break;

//             case 7: meshIndex = 4; rotation = Quaternion.Euler(0, 0, 0); break;    // T
//             case 11: meshIndex = 4; rotation = Quaternion.Euler(0, -90, 0); break;
//             case 13: meshIndex = 4; rotation = Quaternion.Euler(0, 90, 0); break;
//             case 14: meshIndex = 4; rotation = Quaternion.Euler(0, 180, 0); break;

//             case 15: meshIndex = 5; break; // Cross
//             default: meshIndex = 0; break; // Fallback
//         }

//         Vector3 pos = new Vector3(x, y + 0.5f, z); // +0.5f agar di atas tanah (sesuaikan pivot)
//         Matrix4x4 mat = Matrix4x4.TRS(pos, rotation, Vector3.one);
        
//         batchLists[meshIndex].Add(mat);
//     }
    
//     // Helper duplicate untuk cari Y (ideally simpan ini di VoxelWorld biar ga duplikat)
//     int FindSurfaceY(int x, int z)
//     {
//         for (int y = world.worldHeight - 1; y >= 0; y--)
//         {
//             if (world.GetVoxel(x, y, z).isSolid) return y;
//         }
//         return -1;
//     }
// }