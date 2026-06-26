using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Satu pilihan overlay di dalam flyout OverlayPicker.
/// </summary>
[RequireComponent(typeof(Button))]
public class OverlayButton : MonoBehaviour
{
    [SerializeField] private int mode;      // 0=None,1=Risiko,2=Resapan,3=Elevasi
    [SerializeField] private Sprite icon;
    [SerializeField] private OverlayPicker picker;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => picker.Select(mode, icon));
    }
}
