using UnityEngine;
using UnityEngine.InputSystem;

public class ZoneController : MonoBehaviour
{
    [SerializeField] private int currentBudget = 1000;
    [SerializeField] private int brushSize = 1; // Default 0 (1 blok)
    [SerializeField] private int costPerBlock = 10;

    // Referensi ke Database
    private VoxelWorld world;

    public enum PaintTool { Brush, Eraser }
    public PaintTool currentTool;

    // ID untuk Selokan (Misal kita sepakati ID 11 adalah Selokan)
    private byte selectedZoneID = 0;

    // Variable bayangan untuk Gizmos
    private Vector3 lastHitPos;
    private bool isHittingTerrain = false;

    // Dipanggil oleh GameManager
    public void SystemUpdate(VoxelWorld voxelWorld)
    {
        world = voxelWorld;

        // --- INPUT PILIH ZONA ---
        // 1 = Selokan (ID 11), 2 = Jalan (ID 10) - Contoh saja
        if (Keyboard.current.digit0Key.wasPressedThisFrame) selectedZoneID = 0; // Air(Water)
        if (Keyboard.current.digit1Key.wasPressedThisFrame) selectedZoneID = 1; // tanah
        if (Keyboard.current.digit2Key.wasPressedThisFrame) selectedZoneID = 2; // beton
        if (Keyboard.current.digit3Key.wasPressedThisFrame) selectedZoneID = 3; // Industri
        if (Keyboard.current.digit4Key.wasPressedThisFrame) selectedZoneID = 11; // Selokan / perairan

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
                    byte newType = (currentTool == PaintTool.Brush) ? selectedZoneID : (byte)1; // 1 = Tanah

                    VoxelCell currentCell = world.GetVoxel(paintX, surfaceY, paintZ);

                    if (currentCell.blockType != newType)
                    {
                        if (newType == 0) currentCell.amount = 1.0f;
                        // Logic Budget sederhana
                        if (currentTool == PaintTool.Brush && currentBudget >= costPerBlock)
                        {
                            currentBudget -= costPerBlock;
                            currentCell.blockType = newType;
                            world.SetVoxel(paintX, surfaceY, paintZ, currentCell);
                        }
                        else if (currentTool == PaintTool.Eraser)
                        {
                            currentCell.blockType = newType;
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