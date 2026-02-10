using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Global_Building_DB", menuName = "CityBuilder/Building Registry")]
public class BuildingRegistry : ScriptableObject
{
    public enum RenderType { StaticProp, Connectable }

    [System.Serializable]
    public struct BuildingData
    {
        public string name;
        public byte id;
        public Material material;

        [Header("Tipe Rendering")]
        public RenderType renderType; // Pilih: Static atau Connectable?

        [Header("Jika Static Prop")]
        public ZoneType zone;    // Enum ZoneType harus public/ada di luar
        public Mesh mesh;       // Mesh tunggal
        public int width;
        public int depth;
        public Vector3 visualOffset;
        public float visualScale;

        [Header("Jika Connectable (Jalan/Selokan)")]
        // Urutan Array WAJIB: 0:Standalone, 1:End, 2:Straight, 3:Corner, 4:T, 5:Cross
        public Mesh[] connectionMeshes;
        public float yOffset; // Offset tinggi (misal selokan agak turun)
    }

    public List<BuildingData> buildings;

    public BuildingData GetDataByID(byte id)
    {
        foreach (var b in buildings) if (b.id == id) return b;
        return new BuildingData();
    }
}

// Cara Setting di Unity Inspector (Penting!)
// Sekarang Building Registry Anda adalah pusat segalanya.
//     Buka file Global_Building_DB Anda.
//     Tambahkan Jalan (Road):
//         ID: 10 (atau sesuai konstanta Anda).
//         Render Type: Connectable.
//         Connection Meshes: (Size 6). Drag model jalan Anda ke sini. Urutannya HARUS:
//             Element 0: Model Kotak Jalan (Standalone)
//             Element 1: Model Jalan Buntu (End)
//             Element 2: Model Jalan Lurus (Straight)
//             Element 3: Model Tikungan (Corner)
//             Element 4: Model Simpang T (T-Junction)
//             Element 5: Model Simpang 4 (Cross)
//         Y Offset: 0.9 (Misal).
//     Tambahkan Selokan (Gutter):
//         ID: 11.
//         Render Type: Connectable.
//         Connection Meshes: (Size 6). Drag model selokan.
//         Y Offset: 0.8 (Misal lebih rendah dari jalan).
//     Tambahkan Rumah:
//         ID: 40.
//         Render Type: Static Prop.
//         Mesh: Model rumah.

