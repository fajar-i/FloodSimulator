using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Satu pilihan brush di dalam flyout. Saat diklik, memberitahu BrushPicker
/// zona mana yang dipilih beserta ikonnya.
/// </summary>
[RequireComponent(typeof(Button))]
public class BrushButton : MonoBehaviour
{
    [SerializeField] private int zoneId;       // VoxelID zona (30=Hunian, 31=Industri, 32=Tani)
    [SerializeField] private Sprite icon;      // ikon brush ini (untuk dipasang ke induk)
    [SerializeField] private BrushPicker picker;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => picker.Select(zoneId, icon));
    }
}
