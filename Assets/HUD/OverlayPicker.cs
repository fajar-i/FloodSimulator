using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Tombol induk picker OVERLAY (mata) yang mekar saat hover — desain sama
/// dengan BrushPicker. Bedanya: tidak menaruh voxel, hanya mengganti mode
/// visual di OverlayController. Tombol "mata" (gambar 15) = kembali ke
/// tanpa-overlay (ekuivalen Kursor di brush picker).
/// </summary>
public class OverlayPicker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private OverlayController overlayController;
    [SerializeField] private Image indukIcon;
    [SerializeField] private GameObject flyout;

    void Start()
    {
        if (flyout != null) flyout.SetActive(false);
        if (overlayController != null) overlayController.SetNone(); // mulai tanpa overlay
    }

    public void OnPointerEnter(PointerEventData e) { if (flyout != null) flyout.SetActive(true); }
    public void OnPointerExit(PointerEventData e)  { if (flyout != null) flyout.SetActive(false); }

    // mode: 0=None(mata), 1=Risiko, 2=Resapan, 3=Elevasi (sesuai enum OverlayMode).
    public void Select(int mode, Sprite icon)
    {
        if (overlayController != null)
        {
            if (mode <= 0) overlayController.SetNone();
            else           overlayController.SetOverlay((OverlayController.OverlayMode)mode);
        }
        if (indukIcon != null && icon != null) indukIcon.sprite = icon;
        if (flyout != null) flyout.SetActive(false);
    }
}
