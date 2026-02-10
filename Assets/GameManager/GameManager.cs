using UnityEngine.InputSystem;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Initialization, // Generate Terrain
        Planning,       // Player Zoning (Desain Kota)
        Construction,   // Generate Struktur berdasarkan Zone
        Simulation,     // Banjir & Stress Test
        Harvest         // Hitung Resource
    }

    [Header("Modules")]
    public VoxelWorld world;
    public TerrainGenerator terrainGen;
    public CityGameManager cityGameManager;
    public ZoneController zoneController;
    public WaterSimulationSystem waterSystem; // Logic
    // public ChunkedWaterManager disasterController;
    // public EconomyManager economyManager;
    public GameState CurrentState { get; private set; }

    void Start()
    {
        // 1. Inisialisasi Berurutan (Anti-Race Condition)
        world.InitializeWorld();
        ChangeState(GameState.Initialization);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] Entering Phase: {newState}");

        switch (newState)
        {
            case GameState.Initialization:
                terrainGen.GenerateTerrain(world); // Generate Terrain Awal
                ChangeState(GameState.Planning); // Setelah selesai generate, langsung masuk ke fase Planning (walau masih kosong)
                break;

            case GameState.Planning:
                Debug.Log("Fase Planning: Menunggu input player... Tekan ENTER untuk lanjut.");
                break;

            case GameState.Construction:
                Debug.Log("Fase Construction: Membangun gedung...");
                cityGameManager.Initialize();
                break;

            case GameState.Simulation:
                Debug.Log("Fase Simulation: Banjir dimulai!");
                // TO DO: world.isFlooding = true; // Kita bisa akses flag banjir di sini
                break;

            case GameState.Harvest:
                Debug.Log("Fase Harvest: Menghitung resource...");
                // TO DO: economyManager.CalculateResources(world);
                break;
        }
    }

    void NextPhase()
    {
        switch (CurrentState)
        {
            case GameState.Planning:
                zoneController.finish();
                ChangeState(GameState.Construction);
                break;
            case GameState.Construction:
                ChangeState(GameState.Simulation);
                break;
            case GameState.Simulation:
                ChangeState(GameState.Harvest);
                break;
            case GameState.Harvest:
                Debug.Log("Game Loop Selesai. Restart?");
                break;
        }
    }

    void Update()
    {
        // Logic global, misal tombol "Next Phase"
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            NextPhase();
        }
        if (CurrentState == GameState.Planning)
        {
            zoneController.SystemUpdate(world);
        }
        if (CurrentState == GameState.Simulation)
        {
            waterSystem.SystemUpdate(world);
        }
        if (CurrentState != GameState.Planning)
        {
            cityGameManager.OnUpdate(); 
        }
    }
}