using UnityEngine;
using System.Collections;

public class CityGameManager : MonoBehaviour
{
    [Header("Core Database")]
    public VoxelWorld world;
    public BuildingRegistry globalRegistry;

    [Header("Specialists (Subsystems)")]
    public CityGenerator cityGenerator;       // Arsitek Jalan
    public CityAutoBuilder CityAutoBuilder; // Rumah WFC
    public CityRendererSystem cityRenderer;   // Visual

    [Header("Simulation Config")]
    public bool autoBuildOnStart = true;

    private bool isBuilding = false;
    private Coroutine buildCoroutine;

    public void Initialize()
    {
        if (isBuilding)
        {
            Debug.LogWarning("[CityGameManager] Build already in progress. Ignoring Initialize call.");
            return;
        }

        // Pastikan renderer tahu siapa bosnya (World)
        CityAutoBuilder.Initialize(world, globalRegistry);
        cityRenderer.Initialize(world, globalRegistry);

        // 2. MULAI KERJA
        buildCoroutine = StartCoroutine(BuildCityRoutine());
    }
    public void OnUpdate()
    {
        // Panggil update renderer secara manual
        // Ini memastikan renderer jalan SETELAH logika lain selesai
        cityRenderer.OnUpdate();
    }
    // Gunakan Coroutine agar game tidak 'Hang' saat loading
    // dan kita bisa kasih jeda antar fase biar terlihat keren
    IEnumerator BuildCityRoutine()
    {
        isBuilding = true;
        Debug.Log("--- PHASE 1: PREPARING TERRAIN ---");
        // world.GenerateTerrain(); // Jika ada script terrain gen terpisah
        yield return null; // Tunggu 1 frame

        Debug.Log("--- PHASE 2: LAYING ROADS & ZONING ---");
        cityGenerator.GenerateCityLayout(world); // Panggil fungsi public di script CityGenerator
        yield return null;

        Debug.Log("--- PHASE 3: PLACING BUILDINGS ---");
        CityAutoBuilder.BuildEntireCity();
        yield return null;

        Debug.Log("--- PHASE 5: FINALIZING VISUALS ---");
        // Update visual terakhir kali secara menyeluruh
        world.UpdateAllChunks();
        cityRenderer.RebuildAllBatches(); // Setelah semua bangunan ditempatkan, modelnya di render

        Debug.Log("--- CITY BUILT SUCCESSFULLY ---");
        isBuilding = false;
    }

    // Fungsi untuk tombol UI "Rebuild City"
    public void RebuildCity()
    {
        if (buildCoroutine != null)
        {
            StopCoroutine(buildCoroutine);
        }
        isBuilding = false;
        if (world != null)
        {
            world.ClearWorld();
        }
        buildCoroutine = StartCoroutine(BuildCityRoutine());
    }
}