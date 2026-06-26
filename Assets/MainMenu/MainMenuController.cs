using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Mengatur scene MainMenu: highlight item aktif (hover mouse ATAU navigasi keyboard)
/// dan aksi tombol MULAI / PENGATURAN / KELUAR.
/// Default: TIDAK ada item yang aktif. Item baru aktif saat di-hover atau dipilih keyboard.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Tooltip("Nama scene gameplay yang dimuat saat MULAI ditekan. Harus terdaftar di Build Settings.")]
    [SerializeField] private string gameplaySceneName = "_GAME_SYSTEM";

    private MenuItem[] items;       // urutan mengikuti hierarki: MULAI, PENGATURAN, KELUAR
    private Button[] buttons;
    private int keyboardIndex = -1; // -1 = belum ada pilihan keyboard
    private MenuItem hovered;       // item yang sedang di-hover mouse (null = tidak ada)

    void Awake()
    {
        items = GetComponentsInChildren<MenuItem>(true);
        buttons = new Button[items.Length];
        for (int i = 0; i < items.Length; i++) buttons[i] = items[i].GetComponent<Button>();
        RefreshHighlight();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || items.Length == 0) return;

        bool down   = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
        bool up     = kb.upArrowKey.wasPressedThisFrame   || kb.wKey.wasPressedThisFrame;
        bool submit = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;

        if (down)
        {
            keyboardIndex = (keyboardIndex < 0) ? 0 : Mathf.Min(keyboardIndex + 1, items.Length - 1);
            RefreshHighlight();
        }
        else if (up)
        {
            keyboardIndex = (keyboardIndex < 0) ? 0 : Mathf.Max(keyboardIndex - 1, 0);
            RefreshHighlight();
        }

        if (submit)
        {
            int idx = ActiveIndex();
            if (idx >= 0) buttons[idx].onClick.Invoke();
        }
    }

    /// <summary>Index item yang sedang aktif: hover mouse diprioritaskan, lalu pilihan keyboard.</summary>
    private int ActiveIndex()
    {
        if (hovered != null) return System.Array.IndexOf(items, hovered);
        return keyboardIndex; // -1 jika belum ada
    }

    private void RefreshHighlight()
    {
        int active = ActiveIndex();
        for (int i = 0; i < items.Length; i++) items[i].SetActive(i == active);
    }

    public void OnHover(MenuItem item)
    {
        hovered = item;
        keyboardIndex = -1; // mouse mengambil alih: reset pilihan keyboard biar keluar-hover kembali ke none
        RefreshHighlight();
    }
    public void OnHoverExit(MenuItem item) { if (hovered == item) hovered = null; RefreshHighlight(); }

    public void OnMulai()
    {
        Debug.Log("[MainMenu] MULAI ditekan, memuat scene gameplay: " + gameplaySceneName);
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnPengaturan()
    {
        // TODO: Layar Pengaturan belum ada desain/kode (menyusul, bukan prioritas DL Juli).
        Debug.Log("[MainMenu] PENGATURAN belum tersedia (placeholder).");
    }

    public void OnKeluar()
    {
        Debug.Log("[MainMenu] KELUAR ditekan, keluar dari aplikasi.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
