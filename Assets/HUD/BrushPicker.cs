using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Tombol "induk" palet brush yang MEKAR saat hover (fan-out).
/// Dipasang di root area picker. Karena tombol-tombol pilihan adalah anak
/// dari root ini, hover di mana pun dalam area tetap dianggap "di dalam"
/// (event enter/exit menyebar ke induk), jadi menu tidak keburu menutup
/// saat mouse bergeser dari induk ke pilihan.
/// </summary>
public class BrushPicker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ZoneController zoneController;
    [SerializeField] private Image indukIcon;    // ikon di tombol induk (ikut berubah)
    [SerializeField] private GameObject flyout;   // panel berisi pilihan brush

    void Start()
    {
        if (flyout != null) flyout.SetActive(false);               // mulai tertutup
        if (zoneController != null) zoneController.SetCursorMode(); // mulai TANPA brush (mode kursor)
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (flyout != null) flyout.SetActive(true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (flyout != null) flyout.SetActive(false);
    }

    // Dipanggil tiap BrushButton ketika diklik.
    public void Select(int zoneId, Sprite icon)
    {
        if (zoneController != null)
        {
            if (zoneId < 0) zoneController.SetCursorMode();         // -1 = mode kursor (tanpa brush)
            else            zoneController.SetActiveZone((byte)zoneId);
        }
        if (indukIcon != null && icon != null) indukIcon.sprite = icon; // induk jadi pilihan terpilih
        if (flyout != null) flyout.SetActive(false);                     // tutup setelah pilih
    }
}
