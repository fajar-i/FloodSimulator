using UnityEngine;

public class WaterChunk
{
    public GameObject gameObject;
    public Mesh mesh;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    // Koordinat Chunk dalam dunia (misal: 0,0,0 atau 1,0,0)
    public Vector3Int chunkCoord; 
    
    public WaterChunk(Vector3Int coord, Material mat, Transform parent)
    {
        chunkCoord = coord;
        
        // Buat GameObject baru
        gameObject = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
        gameObject.transform.parent = parent;
        // Posisikan GameObject ini sesuai koordinat chunk * ukuran chunk (16)
        gameObject.transform.localPosition = new Vector3(coord.x * 16, coord.y * 16, coord.z * 16);

        mesh = new Mesh();
        mesh.MarkDynamic(); // Penting untuk performa update
        
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mat;
    }
}