using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mengisi label-label di HUD top bar dari data EconomyManager.
///
/// Pola yang dipakai: bukan polling tiap frame, tapi BERLANGGANAN (subscribe)
/// ke event EconomyManager.OnStatsChanged. Jadi label hanya diperbarui saat
/// nilainya benar-benar berubah — hemat & rapi (observer pattern).
/// </summary>
public class HudController : MonoBehaviour
{
    [Header("Sumber Data")]
    [SerializeField] private EconomyManager economy;

    [Header("Label Top Bar")]
    [SerializeField] private Text budgetText;
    [SerializeField] private Text educationText;
    [SerializeField] private Text trustText;
    [SerializeField] private Text weatherText;

    // OnEnable dipanggil Unity saat objek aktif. Tempat ideal untuk subscribe.
    void OnEnable()
    {
        if (economy != null)
            economy.OnStatsChanged += RefreshAll;

        RefreshAll(); // tampilkan nilai awal sekali di depan
    }

    // WAJIB unsubscribe saat nonaktif, agar tidak memory leak / dipanggil objek mati.
    void OnDisable()
    {
        if (economy != null)
            economy.OnStatsChanged -= RefreshAll;
    }

    // Dipanggil otomatis tiap kali EconomyManager memicu OnStatsChanged.
    void RefreshAll()
    {
        if (economy == null) return;

        // Teks ringkas; makna tiap angka diwakili ikon di sebelahnya.
        if (budgetText != null)    budgetText.text    = $"Rp {economy.Budget:n0}";
        if (educationText != null) educationText.text = $"{economy.Education:0}%";
        if (trustText != null)     trustText.text     = $"{economy.Trust:0}%";
        if (weatherText != null)   weatherText.text   = $"{economy.Weather}";
    }
}
