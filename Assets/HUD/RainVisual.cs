using UnityEngine;

/// <summary>
/// Mengatur INTENSITAS visual hujan (Particle System) berdasarkan cuaca di
/// EconomyManager. Murni kosmetik — tidak menyentuh simulasi air (itu tugas
/// WaterSimulationSystem.UpdateRain). Berlangganan OnStatsChanged supaya
/// emisi otomatis berubah saat cuaca ganti, tanpa polling.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class RainVisual : MonoBehaviour
{
    [SerializeField] private EconomyManager economy;
    [Tooltip("Partikel per detik saat Hujan.")]
    [SerializeField] private float hujanRate = 400f;
    [Tooltip("Partikel per detik saat Badai.")]
    [SerializeField] private float badaiRate = 1400f;

    private ParticleSystem ps;

    void Awake() { ps = GetComponent<ParticleSystem>(); }

    void OnEnable()
    {
        if (economy != null) economy.OnStatsChanged += Apply;
        Apply();
    }

    void OnDisable()
    {
        if (economy != null) economy.OnStatsChanged -= Apply;
    }

    void Apply()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();

        float rate = 0f;
        if (economy != null)
        {
            switch (economy.Weather)
            {
                case EconomyManager.WeatherType.Hujan: rate = hujanRate; break;
                case EconomyManager.WeatherType.Badai: rate = badaiRate; break;
                default: rate = 0f; break; // Cerah: tidak ada hujan
            }
        }

        var em = ps.emission;
        em.rateOverTime = rate;

        if (rate > 0f && !ps.isEmitting) ps.Play();
        if (rate <= 0f && ps.isEmitting) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
