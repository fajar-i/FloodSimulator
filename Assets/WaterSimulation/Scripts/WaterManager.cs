using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterManager : MonoBehaviour
{
    [Header("Settings")]
    public int width = 16, height = 32, depth = 16;
    
    [Tooltip("Waktu dalam detik antar update (0.1 = lambat, 0.02 = cepat)")]
    public float simulationDelay = 0.05f; 
    
    [Tooltip("Seberapa banyak air ditambahkan per tick saat spasi ditekan")]
    public float fillRate = 0.5f;

    [Tooltip("Kecepatan air menyebar (Viskositas)")]
    public float flowSpeed = 0.5f;

    NativeArray<VoxelCell> gridA;
    NativeArray<VoxelCell> gridB;
    bool useGridA = true;
    float tickTimer = 0f; // Timer untuk menghitung waktu

    // --- MESH DATA ---
    Mesh mesh;
    MeshFilter meshFilter;
    NativeList<Vector3> meshVertices;
    NativeList<int> meshTriangles;

    void Start()
    {
        int length = width * height * depth;
        gridA = new NativeArray<VoxelCell>(length, Allocator.Persistent);
        gridB = new NativeArray<VoxelCell>(length, Allocator.Persistent);

        mesh = new Mesh();
        mesh.MarkDynamic();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshVertices = new NativeList<Vector3>(length * 24, Allocator.Persistent);
        meshTriangles = new NativeList<int>(length * 36, Allocator.Persistent);
    }

    void Update()
    {
        // 1. Akumulasi Waktu
        tickTimer += Time.deltaTime;

        // 2. Cek apakah sudah waktunya update simulasi?
        if (tickTimer >= simulationDelay)
        {
            // Reset timer (dikurangi delay agar sisa waktu tersimpan)
            tickTimer -= simulationDelay;
            
            // Jalankan 1 langkah simulasi
            RunSimulationStep();
        }
    }

    void RunSimulationStep()
    {
        var read = useGridA ? gridA : gridB;
        var write = useGridA ? gridB : gridA;

        // --- INPUT: KERAN MANUAL ---
        // Menggunakan .isPressed agar air keluar TERUS selama spasi ditahan
        if (Keyboard.current.spaceKey.isPressed)
        {
            // Lokasi sumber air (Tengah Atas)
            int sourceX = width / 2;
            int sourceY = height - 2; 
            int sourceZ = depth / 2;
            int sourceIndex = sourceX + width * (sourceY + height * sourceZ);

            // Ambil data lama, modifikasi, tulis balik
            VoxelCell cell = read[sourceIndex];
            
            // Tambahkan air sedikit demi sedikit (fillRate)
            // math.min memastikan tidak melebihi 1.0f
            cell.amount = math.min(cell.amount + fillRate, 1.0f); 
            cell.isSolid = false;
            
            read[sourceIndex] = cell;
        }

        // --- HAPUS KERAN OTOMATIS YANG LAMA DI SINI ---
        // Kode lama yang 'sCell.amount = 1.0f' dihapus agar air tidak keluar sendiri.

        // Jalankan Job Simulasi
        var simJob = new WaterPullJob
        {
            readGrid = read,
            writeGrid = write,
            size = new int3(width, height, depth),
            flowSpeed = flowSpeed // Gunakan variabel public
        };
        
        JobHandle simHandle = simJob.Schedule(read.Length, 64);
        simHandle.Complete();

        // Swap Buffer
        useGridA = !useGridA;
        var currentDisplayGrid = useGridA ? gridA : gridB;

        // Update Visual Mesh hanya saat simulasi berjalan
        GenerateMesh(currentDisplayGrid);
    }

    void GenerateMesh(NativeArray<VoxelCell> gridToDraw)
    {
        meshVertices.Clear();
        meshTriangles.Clear();

        var meshJob = new WaterMeshJob
        {
            grid = gridToDraw,
            size = new int3(width, height, depth),
            vertices = meshVertices,
            triangles = meshTriangles
        };

        JobHandle meshHandle = meshJob.Schedule();
        meshHandle.Complete();

        mesh.Clear();
        mesh.SetVertices(meshVertices.AsArray());
        mesh.SetIndices(meshTriangles.AsArray(), MeshTopology.Triangles, 0);
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void OnDestroy()
    {
        if (gridA.IsCreated) gridA.Dispose();
        if (gridB.IsCreated) gridB.Dispose();
        if (meshVertices.IsCreated) meshVertices.Dispose();
        if (meshTriangles.IsCreated) meshTriangles.Dispose();
    }
}