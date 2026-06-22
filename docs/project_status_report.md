# Laporan Audit Codebase & Status Proyek (Versi Detail)

Dokumen ini menyajikan hasil penelusuran mendalam terhadap seluruh codebase Unity proyek **Dewan Bengawan**, mengidentifikasi arsitektur teknis yang sudah dibangun, membandingkannya dengan target MVP pada `dewan_bengawan_mvp_handoff.md`, serta merinci status implementasi terkini.

---

## 1. Tinjauan Arsitektur Teknis yang Sudah Terbentuk
Codebase ini memiliki arsitektur yang sangat terstruktur, memadukan sistem fisika berbasis Job System dengan rendering hibrida (Procedural Chunk Mesh + Instanced Props):

### A. Voxel World & Chunking System
- **`VoxelCell.cs`**: Menyimpan state voxel per sel (air `amount`, `isSolid` flag, `rotation` 2-bit, `blockType` ID, serta variabel hidrologi `zoneType`, `absorptionRate`, `flowFriction`, dan `isPollutionSource`).
- **`VoxelID.cs`**: Kelas statis sentral untuk standardisasi ID voxel alam (Water, Grass, Concrete, Rough Ground), infrastruktur (Road, Sewer, Bridge), dan zona/bangunan.
- **`VoxelWorld.cs` & `WaterChunk.cs`**: Membagi world 3D berukuran $32\times32\times32$ menjadi potongan-potongan kecil (Chunk) berukuran $16\times16\times16$ untuk efisiensi update visual.

### B. Dual-Buffer Cellular Automata Water Physics
- **`WaterSimulationSystem.cs` & `WaterSimulationJob.cs`**:
  - Menggunakan pola dual-buffer (`ActiveGrid` dan `NextGrid` NativeArray) untuk menghindari *race conditions* selama kalkulasi paralel.
  - Simulasi dijalankan via Burst-compiled `IJobParallelFor` (`WaterPullJob`) menggunakan tetangga Von Neumann 3D (4 arah horizontal dan 2 arah vertikal untuk gravitasi).
  - Dilengkapi sistem pencegah overshoot aliran air (aliran maksimum dibatasi 25% dari kapasitas saat itu).
  - Dilengkapi dengan *Water Meluap/Banjir* terencana (`RiseFloodLogic`).

### C. Procedural Chunk Mesh Generation
- **`WaterChunkMeshJob.cs`**:
  - Menggunakan sistem *Face Culling* 3D untuk hanya merender permukaan voxel yang bersentuhan dengan udara/transparan (menghemat GPU draw call).
  - Mengimplementasikan kalkulasi koordinat UV atlas tekstur ($4\times4$ grid) secara prosedural untuk pewarnaan tanah/perairan.
  - Voxel solid yang merupakan objek instanced (seperti jalan, selokan, dan gedung) secara otomatis di-cull/skip di sini agar tidak tumpang tindih dengan mesh dasar.

### D. Instanced Prop & Infrastructure Rendering
- **`CityRendererSystem.cs` & `RenderBatch.cs` & `BuildingRegistry.cs`**:
  - Menggunakan `Graphics.DrawMeshInstanced` untuk merender bangunan statis (`StaticProp`) dan infrastruktur yang menyambung (`Connectable`, seperti jalan/selokan).
  - Mengimplementasikan algoritma bitmask universal 4-arah untuk menentukan jenis sambungan mesh jalan/selokan secara dinamis (Standalone, End, Straight, Corner, T-Junction, Cross) dan rotasinya.
  - Dioptimalkan menggunakan reuse pre-allocated batch list untuk mencegah memory stuttering (GC pressure).

---

## 2. Pemetaan Progres Terhadap Target MVP (MoSCoW)

Berdasarkan analisis file sumber, berikut adalah status kesiapan fitur MVP:

### P1 — MUST HAVE (Target Proposal Video)

| Kode | Fitur | Status | Detail di Codebase |
|---|---|---|---|
| **M01** | Zone Painting System | **Selesai** | `ZoneController.cs` mengontrol mouse raycast untuk melukis zona dengan brush/eraser, menyinkronkan buffer, serta menginisialisasi hidrologi voxel secara dinamis menggunakan `VoxelHelper`. |
| **M02** | Hydrological Variables | **Selesai** | Parameter resapan (`absorptionRate`), hambatan gesekan (`flowFriction`), dan `isPollutionSource` berhasil dipetakan secara dinamis berdasarkan tipe blok dan diintegrasikan langsung ke job simulasi air. |
| **M03** | CA Water Simulation | **Selesai** | Berjalan penuh menggunakan Von Neumann 3D paralel dinamis dengan integrasi penyerapan dan gesekan air di `WaterSimulationJob.cs`. |
| **M04** | Procedural City Generation | **Selesai** | `CityGenerator.cs` (jalan & jembatan) dan `CityAutoBuilder.cs` (penempatan gedung pintar dengan perataan tanah cut & fill) bekerja harmonis. |
| **M05** | Dual Heatmap Overlay | **Belum** | Belum ada kode rendering heatmap resapan/elevasi pada permukaan terrain dasar. |
| **M06** | Budget System | **Parsial** | Logika budget sederhana (mengurangi budget per blok) sudah ada di `ZoneController.cs`, namun belum terhubung ke sistem UI/fase global. |
| **M07** | Policy Cards (6 kartu) | **Belum** | ScriptableObject `PolicyCard.cs` dan pengelola `PolicyCardManager.cs` belum dibuat. |
| **M08** | 4-Cycle Game Loop | **Parsial** | GameState enum (`Initialization`, `Planning`, `Construction`, `Simulation`, `Harvest`) sudah didefinisikan di `GameManager.cs` dengan transisi tombol Enter, namun loop 4 siklus penuh belum dikunci. |
| **M09** | Harvest Screen | **Belum** | Belum ada kode UI rapor pendapatan/polusi siklus. |
| **M10** | Insight Card | **Belum** | Belum ada modal edukasi pasca-simulasi. |
| **M11** | Main HUD | **Belum** | Belum ada UI top bar untuk budget, siklus, dll. |
| **M12** | Performance Target | **Selesai** | Optimasi GC batching dan Burst Job menjamin FPS tinggi pada grid $32^3$ (dapat di-scale ke $64\times16\times64$). |

---

## 3. Ketidaksesuaian Teknis & Temuan Kritis (Technical Mismatches)

Seluruh ketidaksesuaian teknis utama yang teridentifikasi pada audit awal telah berhasil diselesaikan:

1. **Penyelarasan Enum `ZoneType`**:
   * *Status*: **Selesai**. Enum `ZoneType` sekarang berpusat secara global di `VoxelCell.cs` sesuai standar handoff (`EMPTY`, `RESIDENTIAL`, `INDUSTRIAL`, `AGRICULTURAL`, `WATER_GREEN`, `WATER_BODY`). Query di `CityAutoBuilder.cs` juga telah di-update.
2. **Kekasaran Permukaan CA Dinamis**:
   * *Status*: **Selesai**. `WaterSimulationJob.cs` sekarang membaca `flowFriction` sel secara dinamis untuk mengkalkulasi kecepatan rambat air lateral (`1.0f - flowFriction`).
3. **Penyimpanan State / Refleksi Akhir**:
   * *Status*: **Belum**. Mekanisme snapshot state `VoxelWorld` per siklus (**B09**) untuk perbandingan performa resapan sebelum vs sesudah simulasi masih pending.

---

## 4. Langkah Pengembangan Selanjutnya (Next Steps)
Untuk melanjutkan progres ke fase berikutnya, langkah berikut disarankan:
1. **Implementasi Policy Card System (Module F)**: Membuat struktur `PolicyCard` ScriptableObject dan manajernya untuk memodifikasi world state secara temporal/global.
2. **Penyusunan UI/UX Layer (Module H)**: Membuat UI dasar untuk HUD, menu pemilihan zona, deck kartu kebijakan, dan harvest report screen.
3. **Visual Heatmap Overlay (Module D)**: Implementasi render shader/material untuk menampilkan visualisasi resapan (absorption) dan elevasi di atas terrain dasar.
