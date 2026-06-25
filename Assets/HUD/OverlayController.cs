using UnityEngine;

/// <summary>
/// Menyimpan mode overlay visual yang sedang aktif (mata = lihat data peta).
/// Sekarang baru menyimpan STATE-nya; rendering overlay (heatmap) menyusul.
/// </summary>
public class OverlayController : MonoBehaviour
{
    public enum OverlayMode { None, Risiko, Resapan, Elevasi }

    [SerializeField] private OverlayMode current = OverlayMode.None;
    public OverlayMode Current => current;

    public event System.Action OnOverlayChanged;

    public void SetOverlay(OverlayMode mode)
    {
        if (current == mode) return;
        current = mode;
        OnOverlayChanged?.Invoke();
        Debug.Log($"[OverlayController] Overlay aktif: {mode}");
        // TODO: render overlay (heatmap Risiko / Resapan / Elevasi) — sistem render belum ada.
    }

    public void SetNone() => SetOverlay(OverlayMode.None);
}
