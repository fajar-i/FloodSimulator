[System.Serializable]
public struct VoxelCell
{
    public float amount; // 0.0 - 1.0 (Air)
    public bool isSolid; // True = Tanah/Gedung
    public int absorption;
    
    public byte blockType; // 0 = Udara, 1 = Tanah/Rumput, 2 = Beton/Gedung, 3 = Batu/Industri
}