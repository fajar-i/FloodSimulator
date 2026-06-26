using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Tampilan satu item menu (MULAI/PENGATURAN/KELUAR).
/// Hanya "view": status aktif/non-aktif diatur oleh <see cref="MainMenuController"/>.
/// Aktif: teks HITAM + kapsul kuning tampil. Non-aktif: teks PUTIH (#D9D9D9) + kapsul disembunyikan.
/// Hover mouse dilaporkan ke controller; tidak menyimpan state sendiri.
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Kapsul kuning (menu_holder) di belakang teks.")]
    [SerializeField] private Image holder;
    [Tooltip("Teks label item (font Times New Roman Bold).")]
    [SerializeField] private Text label;

    [SerializeField] private Color colorAktif = Color.black;
    [SerializeField] private Color colorNonAktif = new Color(0.851f, 0.851f, 0.851f); // #D9D9D9

    private MainMenuController controller;

    void Awake()
    {
        controller = GetComponentInParent<MainMenuController>();
        SetActive(false); // default: tidak ada yang aktif
    }

    public void SetActive(bool on)
    {
        if (holder != null) holder.enabled = on;
        if (label != null) label.color = on ? colorAktif : colorNonAktif;
    }

    public void OnPointerEnter(PointerEventData e) { if (controller != null) controller.OnHover(this); }
    public void OnPointerExit(PointerEventData e) { if (controller != null) controller.OnHoverExit(this); }
}
