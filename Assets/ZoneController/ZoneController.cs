using UnityEngine;
using UnityEngine.InputSystem;

public class ZoneController : MonoBehaviour
{
    [SerializeField] private int currentBudget = 1000000000;
    [SerializeField] private int brushSize = 1; // Default 0 (1 blok)
    [SerializeField] private int costPerBlock = 1;

    // Referensi ke Database
    private VoxelWorld world;

    public enum PaintTool { Brush, Eraser }
    public PaintTool currentTool;

    // ID untuk Selokan (Misal kita sepakati ID 11 adalah Selokan)
    private byte selectedZoneID = 1; //default tanah

    // Variable bayangan untuk Gizmos
    private Vector3 lastHitPos;
    private bool isHittingTerrain = false;

    // Dipanggil oleh GameManager
    public void SystemUpdate(VoxelWorld voxelWorld)
    {
        world = voxelWorld;

        // --- INPUT PILIH ZONA ---
        if (Keyboard.current.digit0Key.wasPressedThisFrame) selectedZoneID = VoxelID.WATER;             // Air = 0
        if (Keyboard.current.digit1Key.wasPressedThisFrame) selectedZoneID = VoxelID.GRASS;             // Rumput / Tanah = 1
        if (Keyboard.current.digit2Key.wasPressedThisFrame) selectedZoneID = VoxelID.CONCRETE;          // Beton = 2
        if (Keyboard.current.digit3Key.wasPressedThisFrame) selectedZoneID = VoxelID.ZONE_RESIDENTIAL; // Zona Perumahan = 30
        if (Keyboard.current.digit4Key.wasPressedThisFrame) selectedZoneID = VoxelID.ZONE_INDUSTRIAL;  // Zona Industri = 31
        if (Keyboard.current.digit5Key.wasPressedThisFrame) selectedZoneID = VoxelID.ZONE_AGRICULTURAL;  // Zona Pertanian = 32

        // --- LOGIKA RAYCAST ---
        var cursorposition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(cursorposition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            lastHitPos = hit.point;
            isHittingTerrain = true;

            // Geser sedikit masuk ke dalam blok agar akurat
            Vector3 pointInBlock = hit.point + (ray.direction * 0.01f);

            int targetX = Mathf.FloorToInt(pointInBlock.x);
            int targetZ = Mathf.FloorToInt(pointInBlock.z);

            if (Mouse.current.leftButton.isPressed)
            {
                ApplyPaint(targetX, targetZ);
            }
        }
        else
        {
            isHittingTerrain = false;
        }
    }

    public void finish()
    {
        if (world == null) return;

        int totalVoxels = world.worldWidth * world.worldHeight * world.worldDepth;
        bool changed = false;

        for (int i = 0; i < totalVoxels; i++)
        {
            VoxelCell cell = world.ActiveGrid[i];
            if (cell.blockType == 0 && cell.isSolid) // Jika tipe air tapi masih padat (solid)
            {
                VoxelCell newcell = cell;
                newcell.isSolid = false;
                world.ActiveGrid[i] = newcell;
                world.NextGrid[i] = newcell;
                changed = true;
            }
        }

        if (changed)
        {
            Debug.Log("[ZoneController] Water solids unlocked. Rebuilding all meshes...");
            world.UpdateAllChunks();
        }
    }

    void ApplyPaint(int centerX, int centerZ)
    {
        if (world == null) return;

        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int z = -brushSize; z <= brushSize; z++)
            {
                int paintX = centerX + x;
                int paintZ = centerZ + z;

                int surfaceY = FindSurfaceY(paintX, paintZ);

                if (surfaceY != -1)
                {
                    // Logic Brush/Eraser
                    byte newType = (currentTool == PaintTool.Brush) ? selectedZoneID : VoxelID.GRASS; // Default = Rumput

                    VoxelCell currentCell = world.GetVoxel(paintX, surfaceY, paintZ);

                    if (currentCell.blockType != newType)
                    {
                        // Logic Budget sederhana
                        if (currentTool == PaintTool.Brush && currentBudget >= costPerBlock)
                        {
                            currentBudget -= costPerBlock;
                            VoxelHelper.InitializeHydrology(ref currentCell, newType);
                            if (newType == VoxelID.WATER) currentCell.amount = 1.0f;
                            world.SetVoxel(paintX, surfaceY, paintZ, currentCell);
                        }
                        else if (currentTool == PaintTool.Eraser)
                        {
                            VoxelHelper.InitializeHydrology(ref currentCell, newType);
                            world.SetVoxel(paintX, surfaceY, paintZ, currentCell);
                        }
                    }
                }
            }
        }
    }

    int FindSurfaceY(int x, int z)
    {
        if (world == null) return -1;
        for (int y = world.worldHeight - 1; y >= 0; y--)
        {
            VoxelCell cell = world.GetVoxel(x, y, z);
            if (cell.isSolid) return y;
        }
        return -1;
    }
}