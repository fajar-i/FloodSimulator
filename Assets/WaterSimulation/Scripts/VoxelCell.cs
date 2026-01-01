[System.Serializable]
public struct VoxelCell
{
    public float amount; // 0.0 - 1.0 (Air)
    public bool isSolid; // True = Tanah/Gedung
    
    // 0 = Udara, 1 = Tanah/Rumput, 2 = Beton/Gedung, 3 = Batu/Industri
    public byte blockType; 
}