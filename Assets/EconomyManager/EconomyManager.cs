using UnityEngine;

/// <summary>
/// Sumber kebenaran (single source of truth) untuk status kota yang ditampilkan
/// di HUD top bar: Anggaran, Pendidikan, Kepercayaan, dan Cuaca.
///
/// Konvensi proyek: subsistem TIDAK meng-update dirinya sendiri di Update().
/// GameManager yang memanggil SystemUpdate(world) di fase yang tepat.
/// (lihat catatan di CLAUDE.md bagian "Orchestration & subsystem convention")
/// </summary>
public class EconomyManager : MonoBehaviour
{
    // Ramalan cuaca BMKG. Mempengaruhi intensitas banjir saat fase Simulation nanti.
    public enum WeatherType { Cerah, Hujan, Badai }

    [Header("Status Kota (dibaca oleh HUD)")]
    [Tooltip("Anggaran kota dalam Rupiah.")]
    [SerializeField] private long budget = 1_000_000_000; // 1 Milyar (samakan dgn ZoneController utk sementara)

    [Tooltip("Tingkat pendidikan warga, ditampilkan sebagai persen 0-100%.")]
    [SerializeField, Range(0f, 100f)] private float education = 50f;

    [Tooltip("Kepercayaan masyarakat ke pemerintah, persen 0-100%.")]
    [SerializeField, Range(0f, 100f)] private float trust = 50f;

    [Tooltip("Ramalan cuaca saat ini.")]
    [SerializeField] private WeatherType weather = WeatherType.Cerah;

    // ----- Akses READ-ONLY untuk modul lain / HUD -----
    // Kode lain boleh membaca nilai, tapi tidak boleh mengubahnya langsung.
    // Perubahan WAJIB lewat method di bawah agar selalu ter-clamp & memicu event.
    public long Budget => budget;
    public float Education => education;
    public float Trust => trust;
    public WeatherType Weather => weather;

    /// <summary>
    /// Dipicu setiap kali salah satu status berubah.
    /// Nanti HUD "berlangganan" event ini supaya label otomatis ter-update,
    /// tanpa harus polling nilai tiap frame.
    /// </summary>
    public event System.Action OnStatsChanged;

    // Dipanggil oleh GameManager setiap frame.
    // Untuk sekarang masih kosong (placeholder); nanti diisi logika seperti
    // pemasukan pasif, progres cuaca, atau dampak hasil simulasi banjir.
    public void SystemUpdate(VoxelWorld world)
    {
        // TODO: logika ekonomi/cuaca per-frame menyusul.
    }

    // ==========================================================
    //  API MUTASI — satu-satunya pintu untuk mengubah status kota
    // ==========================================================

    /// <summary>
    /// Coba belanjakan sejumlah uang. Return false (dan tidak mengubah apa pun)
    /// kalau anggaran tidak cukup. Pola "Try..." supaya pemanggil bisa cek dulu.
    /// </summary>
    public bool TrySpend(long amount)
    {
        if (amount <= 0) return true;      // tidak ada yang dibelanjakan
        if (budget < amount) return false; // dana tidak cukup

        budget -= amount;
        OnStatsChanged?.Invoke();
        return true;
    }

    /// <summary>Menambah anggaran (mis. pemasukan pajak / hasil panen).</summary>
    public void AddBudget(long amount)
    {
        if (amount == 0) return;
        budget += amount;
        OnStatsChanged?.Invoke();
    }

    /// <summary>Set tingkat pendidikan (otomatis dibatasi 0-100).</summary>
    public void SetEducation(float percent)
    {
        education = Mathf.Clamp(percent, 0f, 100f);
        OnStatsChanged?.Invoke();
    }

    /// <summary>Set kepercayaan masyarakat (otomatis dibatasi 0-100).</summary>
    public void SetTrust(float percent)
    {
        trust = Mathf.Clamp(percent, 0f, 100f);
        OnStatsChanged?.Invoke();
    }

    /// <summary>Ganti ramalan cuaca.</summary>
    public void SetWeather(WeatherType newWeather)
    {
        if (weather == newWeather) return;
        weather = newWeather;
        OnStatsChanged?.Invoke();
    }
}
