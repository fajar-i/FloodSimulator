using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// WAJIB: Tambahkan : MonoBehaviour agar dianggap komponen Unity
public class ZoneController : MonoBehaviour
{
    [SerializeField] private int currentBudget = 1000;
    [SerializeField] private int brushSize = 1;
    [SerializeField] private int costPerBlock = 10;

    // Kita cari referensi ini otomatis nanti
    private VoxelWorld world;

    public enum PaintTool { Brush, Eraser }
    public PaintTool currentTool;

    // Default ID (misal 2 = Urban)
    private byte selectedZoneID = 1;

    // Variable bayangan untuk Gizmos
    private Vector3 lastHitPos;
    private bool isHittingTerrain = false;

    public void SystemUpdate(VoxelWorld voxelWorld)
    {
        world = voxelWorld;
        // --- INPUT PILIH ZONA (1-5) ---
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            selectedZoneID = 1;
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            selectedZoneID = 2;
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            selectedZoneID = 3;
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            selectedZoneID = 4;
        }
        else if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            selectedZoneID = 5;
        }
        // Debug.Log(selectedZoneID);


        // --- LOGIKA RAYCAST ---
        var cursorposition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(cursorposition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            lastHitPos = hit.point;
            isHittingTerrain = true;

            // Trik kecil: Geser sedikit ke bawah (-0.01) agar yakin kena blok yang dimaksud, bukan blok di atasnya
            Vector3 pointInBlock = hit.point + (ray.direction * 0.01f);

            int targetX = Mathf.FloorToInt(pointInBlock.x);
            int targetZ = Mathf.FloorToInt(pointInBlock.z);

            if (Mouse.current.leftButton.isPressed)
            {
                ApplyPaint(targetX, targetZ);
                // Tidak perlu panggil MarkChunkDirty manual lagi
                // karena sudah ditangani oleh SetVoxel di dalam ApplyPaint
            }
        }
        else
        {
            isHittingTerrain = false;
        }
    }

    // --- LOGIKA VISUALISASI (GIZMOS) ---
    // Fungsi ini dipanggil otomatis oleh Unity, bahkan saat game tidak Play (kalau pakai OnDrawGizmos)
    // Tapi karena kita pakai data dari Update (lastHitPos), ini akan muncul saat Play.
    // void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying) return; // Hanya gambar saat Play Mode
    //     if (!isHittingTerrain) return; // Jika mouse tidak kena tanah, jangan gambar

    //     // 1. Tentukan Warna
    //     // Hijau kalau Brush, Merah kalau Eraser
    //     Gizmos.color = (currentTool == PaintTool.Brush) ? Color.green : Color.red;

    //     // 2. Hitung Pusat Grid
    //     // Kita snap ke grid integer agar pas dengan voxel
    //     float snapX = Mathf.Floor(lastHitPos.x) + 0.5f; // +0.5 agar pas di tengah kotak 1x1
    //     float snapY = Mathf.Floor(lastHitPos.y) + 1.0f; // Sedikit di atas tanah
    //     float snapZ = Mathf.Floor(lastHitPos.z) + 0.5f;
    //     Vector3 center = new Vector3(snapX, snapY, snapZ);

    //     // 3. Hitung Ukuran Kotak
    //     // Rumus: (Size * 2) + 1. 
    //     // Size 0 -> 1x1. Size 1 -> 3x3. Size 2 -> 5x5.
    //     float size = (brushSize * 2) + 1;
    //     Vector3 cubeSize = new Vector3(size, 0.2f, size); // Tipis saja (0.2f)

    //     // 4. Gambar Kawat Kotak
    //     Gizmos.DrawWireCube(center, cubeSize);

    //     // (Opsional) Gambar kotak transparan biar lebih jelas
    //     Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
    //     Gizmos.DrawCube(center, cubeSize);
    // }

    void ApplyPaint(int centerX, int centerZ)
    {
        if (world == null) return;

        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int z = -brushSize; z <= brushSize; z++)
            {
                int paintX = centerX + x;
                int paintZ = centerZ + z;

                // Cari permukaan tanah
                int surfaceY = FindSurfaceY(paintX, paintZ);

                if (surfaceY != -1)
                {
                    // Tentukan mau jadi apa
                    byte newType = (currentTool == PaintTool.Brush) ? selectedZoneID : (byte)1; // 1 = Default Tanah

                    // Ambil data lama
                    VoxelCell currentCell = world.GetVoxel(paintX, surfaceY, paintZ);

                    // Cek agar tidak buang duit di tempat yang sama
                    if (currentCell.blockType != newType)
                    {
                        if (currentTool == PaintTool.Brush)
                        {
                            if (currentBudget >= costPerBlock)
                            {
                                currentBudget -= costPerBlock;
                                currentCell.blockType = newType;
                                world.SetVoxel(paintX, surfaceY, paintZ, currentCell);
                            }
                        }
                        else // Eraser
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
        // Loop dari atas ke bawah
        for (int y = world.worldHeight - 1; y >= 0; y--)
        {
            VoxelCell cell = world.GetVoxel(x, y, z);
            if (cell.isSolid)
            {
                return y;
            }
        }
        return -1;
    }
}