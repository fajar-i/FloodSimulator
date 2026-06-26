using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimap "PETA KOTA": memindai VoxelWorld dari atas dan melukis Texture2D
/// kecil (1 piksel per kolom x,z) yang diwarnai berdasarkan zoneType.
/// Read-only terhadap dunia; tidak mengubah apa pun.
/// </summary>
public class MinimapController : MonoBehaviour
{
    [SerializeField] private RawImage targetImage;
    [SerializeField] private float refreshInterval = 0.5f;

    private VoxelWorld world;
    private Texture2D tex;
    private float timer;

    void Start()
    {
        world = FindFirstObjectByType<VoxelWorld>();
        if (world != null)
        {
            tex = new Texture2D(world.worldWidth, world.worldDepth, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // tegas/pixelated, bukan blur
            if (targetImage != null) targetImage.texture = tex;
        }
    }

    void Update()
    {
        if (world == null || tex == null) return;
        if (!world.ActiveGrid.IsCreated) return; // dunia belum siap

        timer += Time.unscaledDeltaTime;
        if (timer >= refreshInterval) { timer = 0f; Refresh(); }
    }

    void Refresh()
    {
        int w = world.worldWidth, d = world.worldDepth, h = world.worldHeight;
        for (int z = 0; z < d; z++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, z, ColorOf(x, z, h));
        tex.Apply();
    }

    // Warna kolom (x,z) = voxel permukaan teratas.
    private Color ColorOf(int x, int z, int h)
    {
        for (int y = h - 1; y >= 0; y--)
        {
            VoxelCell c = world.GetVoxel(x, y, z);
            if (c.amount > 0.3f && c.blockType == VoxelID.WATER)
                return new Color(0.30f, 0.50f, 0.90f); // air
            if (!c.isSolid) continue;

            switch (c.zoneType)
            {
                case ZoneType.RESIDENTIAL:  return new Color(0.95f, 0.80f, 0.30f); // Hunian (kuning)
                case ZoneType.INDUSTRIAL:   return new Color(0.90f, 0.45f, 0.30f); // Industri (oranye)
                case ZoneType.AGRICULTURAL: return new Color(0.40f, 0.75f, 0.35f); // Tani (hijau)
                case ZoneType.WATER_GREEN:  return new Color(0.30f, 0.70f, 0.60f); // Resapan (teal)
                case ZoneType.WATER_BODY:   return new Color(0.30f, 0.50f, 0.90f); // Air
                default:                    return new Color(0.72f, 0.70f, 0.60f); // tanah/lainnya
            }
        }
        return new Color(0.18f, 0.20f, 0.23f); // kosong
    }
}
