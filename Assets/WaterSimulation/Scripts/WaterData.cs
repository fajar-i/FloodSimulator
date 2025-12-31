using Unity.Mathematics;

[System.Serializable]
public struct VoxelCell
{
    public float amount; // 0.0 (Kering) s/d 1.0 (Penuh)
    public bool isSolid; // True = Tanah/Batu, False = Udara/Air
}