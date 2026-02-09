[System.Serializable]
public struct VoxelCell
{
    public float amount; // 0.0 - 1.0 (Air)
    public bool isSolid; // True = Tanah/Gedung
    public int absorption;
    public byte rotation;  // [BARU] 0=0, 1=90, 2=180, 3=270
    public byte blockType; // 0 = Air(Water), 1 = Tanah/Rumput, 2 = Beton/Gedung, 3 = Batu/Industri
}

// ID 0-9 (Alam): Udara, Air, Tanah, Pasir.

// ID 10-29 (Infrastruktur/Skeleton):

//     10: Jalan (Bitmask)

//     11: Selokan (Bitmask)

//     12: Jembatan

// ID 30-39 (Zona/Marker):

//     30: Zona Perumahan (Lantai Hijau Transparan)

//     31: Zona Industri (Lantai Kuning Transparan)

// ID 40+ (Bangunan Jadi/WFC Result):

//     40: Rumah Kecil

//     41: Pabrik

//     dst.