using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tombol "Mulai Simulasi" di pojok kanan-bawah. Memanggil
/// GameManager.NextPhase() saat diklik (setara menekan Enter).
/// Label tombol menyesuaikan fase berikutnya supaya satu tombol berguna
/// di seluruh alur (Planning → Construction → Simulation → Harvest).
/// Read-only terhadap simulasi; hanya memicu transisi fase.
/// </summary>
[RequireComponent(typeof(Button))]
public class PhaseButton : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Text label;

    void Start()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        GetComponent<Button>().onClick.AddListener(OnClick);
        RefreshLabel();
    }

    void Update()
    {
        // Murah: hanya menyetel teks bila fase berubah.
        RefreshLabel();
    }

    void OnClick()
    {
        if (gameManager != null) gameManager.NextPhase();
    }

    void RefreshLabel()
    {
        if (label == null || gameManager == null) return;
        string t;
        switch (gameManager.CurrentState)
        {
            case GameManager.GameState.Planning:     t = "Mulai Konstruksi"; break;
            case GameManager.GameState.Construction: t = "Mulai Simulasi";   break;
            case GameManager.GameState.Simulation:   t = "Panen";            break;
            case GameManager.GameState.Harvest:      t = "Selesai";          break;
            default:                                 t = "Mulai Simulasi";   break;
        }
        if (label.text != t) label.text = t;
    }
}
