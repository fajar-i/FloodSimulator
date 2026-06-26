# Dewan Bengawan — MVP & Development Handoff Document

> **Versi**: 1.1 · **Terakhir diperbarui**: 18 Juni 2026 · **Owner dokumen**: Fajari (Ketua Tim)
>
> **Cara pakai**: Paste ke Notion (checkbox otomatis aktif) atau GitHub Projects. Update status task secara rutin. Setiap CP perlu sign-off ketua sebelum lanjut ke fase berikutnya.

---

## Quick Reference — Checkpoint Timeline

| Checkpoint | Target Minggu | Milestone | Tujuan |
|---|---|---|---|
| **CP1** | Minggu 2 | Foundation Lock | Infrastruktur tim & simulasi dasar jalan |
| **CP2** | Minggu 6 | Core Loop Demo-able | 1 siklus penuh bisa dimainkan |
| **CP3** | Minggu 8 | Proposal Video Cut | ≥50% fungsional, siap rekam video |
| **CP4** | Minggu 12 | Feature Complete | Semua fitur MVP selesai, siap pilot |
| **CP5** | Minggu 14 | Pilot-Ready & Final | Build pasca-iterasi pilot, siap tahap final |

---

## 1. MVP Scope (MoSCoW)

### P1 — MUST HAVE (Wajib ada di CP3 — Proposal Video)

| # | Fitur | Deskripsi |
|---|---|---|
| M01 | Zone Painting System | Brush tool 5 tipe zona (Industri, Pemukiman, Pertanian, Perairan, Tanah Kosong) |
| M02 | Hydrological Variables | Mapping zona → absorptionRate, flowFriction, pollutionSource per sel |
| M03 | CA Water Simulation | Simulasi aliran air berbasis Cellular Automata, Von Neumann 3D, Job System |
| M04 | Procedural City Generation | Auto-spawn bangunan saat zona dicat, variasi rotasi & skala |
| M05 | Dual Heatmap Overlay | Overlay resapan + overlay elevasi, bisa toggle |
| M06 | Budget System | Anggaran terbatas, terpotong saat zonasi |
| M07 | Policy Cards (6 kartu) | Minimal 6 kartu fungsional: Biopori, Restorasi Drainase, Subak, Terasering, Lubuk Larangan, Sasi |
| M08 | 4-Cycle Game Loop | Loop Plan→Build→Simulate→Harvest berjalan 4 siklus |
| M09 | Harvest Screen (minimal) | Rapor siklus: pendapatan, polusi, resume siklus berikutnya |
| M10 | Insight Card (minimal) | Modal edukatif muncul setelah simulasi, minimal 4 konten (1/siklus) |
| M11 | Main HUD | Top bar: budget, cycle counter, weather forecast |
| M12 | Performance Target | ≥30 FPS pada grid 64×16×64 minimum di laptop mid-range |

### P2 — SHOULD HAVE (Wajib ada di CP4 — Pilot Test)

| # | Fitur | Deskripsi |
|---|---|---|
| S01 | Policy Cards (12 kartu) | Semua 12 kartu fungsional |
| S02 | Reputation × Public Education | Sistem 2-axis yang memodulasi efektivitas kartu |
| S03 | Final Report Screen | Rapor 4 siklus dengan grafik resilience, ekonomi, polusi |
| S04 | Reflection & Export Screen | Prompt refleksi tertulis + ekspor peta (screenshot) |
| S05 | Tutorial Overlay | 4-step tutorial interaktif (brush, zona, simulasi, kartu) |
| S06 | Stress Test Grid | Simulasi stabil di grid 128×20×128 ≥30 FPS |
| S07 | Hero Indonesian Assets | 5 model 3D bespoke (rumah panggung, lumbung, terasering, subak, pasar) |
| S08 | Critical Zone Alert | Indikator visual zona kritis saat simulasi |
| S09 | NPC Dialog (simplified) | Dialog teks statis dari NPC sesepuh, tanpa branching |
| S10 | Save System | Simpan & lanjut sesi permainan |
| S11 | Analysis Mode Tab | 3 layer heatmap: resapan, elevasi, risiko banjir |
| S12 | Mini-map | Overview grid di pojok bawah, collapsible |

### P3 — WON'T HAVE (V2 / Post-LIDM)

| # | Fitur | Alasan Ditunda |
|---|---|---|
| W01 | Story Mode (Farwan & Dauri) | Terlalu besar, bukan core educational value |
| W02 | NPC Dialog Branching | Dev cost tinggi, tidak menambah learning outcome utama |
| W03 | Achievement System | Nice-to-have, tidak memengaruhi skor LIDM |
| W04 | New Game+ | Post-pilot feature |
| W05 | Mobile/Tablet Support | Di luar platform target |
| W06 | Multiplayer / Co-op | Skope berbeda |

---

## 2. Kill Switches

Jika pada minggu ke-10 fitur berikut belum 70% selesai, **aktifkan kill switch** dan lanjutkan tanpa fitur tersebut daripada menunda seluruh milestone.

| Fitur | Kill Switch Aktif Jika | Fallback |
|---|---|---|
| Reputation 2-axis | Tidak selesai di minggu 10 | Tampilkan sebagai info-only meter, tanpa efek mekanik |
| NPC Dialog | Tidak selesai di minggu 10 | Hapus; ganti dengan text popup statis |
| Hero Assets (5 model) | Tidak selesai di minggu 9 | Pakai Kenney CC0 + disclaimer; tambah V2 |
| Mini-map | Tidak selesai di minggu 11 | Hapus; heatmap sudah cukup untuk analisis spasial |
| Save System | Tidak selesai di minggu 11 | Single-session only; note di proposal |

---

## 3. Tasklist by Module

**Format setiap task**:
`- [ ] [KODE] Deskripsi task — @Owner — Effort — [PRIORITAS]`

**Effort**: XS=1-2 jam · S=setengah hari · M=1 hari · L=2-3 hari · XL=4-5 hari
**Prioritas**: [P1]=Must Have · [P2]=Should Have

---

### MODULE A — Core Simulation Engine
> Owner utama: **@Fajari** · File terkait: `WaterSimulationJob.cs`, `WaterSimulationSystem.cs`, `VoxelCell.cs`, `VoxelWorld.cs`

- [ ] **A01** Audit & refactor `WaterSimulationJob.cs`: pastikan NativeArray sudah dipakai untuk grid data, hapus dead code — @Fajari — S — [P1]
- [ ] **A02** Implementasi Von Neumann 3D Neighborhood (6 arah) dalam aturan transisi CA di `WaterSimulationJob.cs` — @Fajari — L — [P1]
- [ ] **A03** Implementasi `absorptionRate` per sel: air berkurang dari `waterLevel` berdasarkan nilai absorption — @Fajari — M — [P1]
- [ ] **A04** Implementasi `flowFriction` per sel: konstrain kecepatan perpindahan air antar sel — @Fajari — M — [P1]
- [ ] **A05** Implementasi gravitasi vertikal: air jatuh ke sel di bawah bila sel tersebut memiliki `waterLevel` lebih rendah — @Fajari — M — [P1]
- [ ] **A06** Implementasi presipitasi (input hujan): distribusi air ke seluruh sel permukaan berdasarkan nilai intensitas hujan — @Fajari — S — [P1]
- [ ] **A07** Implementasi `pollutionSource` dan `pollutionSpread`: sel industri menandai polusi, polusi menyebar bersama aliran air — @Fajari — L — [P1]
- [ ] **A08** Scheduling Job System: pastikan `WaterSimulationSystem` menjalankan jobs secara paralel tanpa block main thread — @Fajari — M — [P1]
- [ ] **A09** Burst Compiler annotation: tambahkan `[BurstCompile]` pada semua job yang eligible, verifikasi tidak ada managed objects — @Fajari — S — [P1]
- [ ] **A10** Benchmark simulasi: ukur FPS pada grid 64³ dan 128³, catat hasilnya sebagai baseline performa — @Fajari — S — [P1]
- [ ] **A11** Tuning parameter CA: kalibrasi koefisien agar perilaku air terasa natural (tidak terlalu cepat/lambat) — @Fajari + @Syukri — L — [P1]
- [ ] **A12** Stress test 10 menit pada grid 128×20×128: pastikan tidak ada memory leak atau FPS degradasi bertahap — @Fajari — M — [P2]

---

### MODULE B — Zone & World Management
> Owner utama: **@Fajari** · File terkait: `ZoneController.cs`, `VoxelCell.cs`, `VoxelWorld.cs`

- [ ] **B01** Definisi `ZoneType` enum: `EMPTY`, `RESIDENTIAL`, `INDUSTRIAL`, `AGRICULTURAL`, `WATER_GREEN`, `WATER_BODY` — @Fajari — XS — [P1]
- [ ] **B02** Tambahkan field zona ke `VoxelCell`: `zoneType`, `absorptionRate`, `flowFriction`, `isPollutionSource` — @Fajari — S — [P1]
- [ ] **B03** Implementasi brush input handler: klik/drag pada grid mengubah `zoneType` sel terkait — @Fajari — M — [P1]
- [ ] **B04** Mapping tabel zona → nilai hidrologi: saat zona diubah, set otomatis nilai absorpsi & friction sesuai tabel parameter — @Fajari + @Syukri — M — [P1]
- [ ] **B05** Budget deduction: setiap perubahan zona mengurangi budget; jika budget habis, brush dinonaktifkan — @Fajari — S — [P1]
- [ ] **B06** Zone color feedback: warna permukaan tanah berubah instan saat zona dicat (visual confirmation) — @Fajari + @Alvin — S — [P1]
- [ ] **B07** Eraser tool: brush khusus yang mengembalikan sel ke `EMPTY` dan reset nilai hidrologi — @Fajari — S — [P1]
- [ ] **B08** Batasi zona industri: tambahkan aturan bahwa industri tidak boleh ditempatkan langsung di tepi sel `WATER_BODY` (validasi topologi) — @Fajari — M — [P2]
- [ ] **B09** Simpan state grid per siklus: snapshot `VoxelWorld` sebelum simulasi untuk keperluan comparison & export — @Fajari — M — [P2]

---

### MODULE C — Procedural City Generation
> Owner utama: **@Alvin** · File terkait: `CityAutoBuilder.cs`, `CityGenerator.cs`, `BuildingRegistry.cs`, `Global_Building_DB.asset`

- [ ] **C01** Refactor `BuildingRegistry.cs`: tambahkan field `zoneType` ke setiap building entry agar spawner bisa filter per zona — @Alvin — S — [P1]
- [ ] **C02** Implementasi building spawner: saat sel di-zone, spawn 1 model dari pool yang sesuai `zoneType`-nya — @Alvin — M — [P1]
- [ ] **C03** Randomisasi spawn: variasi rotasi (0°, 90°, 180°, 270°) dan skala kecil (±10%) per building instance — @Alvin — S — [P1]
- [ ] **C04** Density logic: jika 3+ sel zona sama bersebelahan, spawn varian padat (rumah petak, bukan rumah tunggal) — @Alvin — M — [P1]
- [ ] **C05** Despawn saat zona berubah: saat sel di-re-zone atau dihapus, remove building lama dan spawn baru yang sesuai — @Alvin — M — [P1]
- [ ] **C06** GPU Instancing untuk buildings: batching semua instance satu zona dalam single draw call via `RenderBatch.cs` — @Alvin — L — [P1]
- [ ] **C07** Integrasi 5 hero assets Indonesia ke `Global_Building_DB.asset` saat model selesai dibuat — @Alvin — S — [P2]
- [ ] **C08** LOD sederhana: model simplifikasi pada zoom jauh untuk menjaga FPS — @Alvin — M — [P2]

---

### MODULE D — Rendering & Visual Feedback
> Owner utama: **@Alvin** · File terkait: `WaterMeshJob.cs`, `TerrainGenerator.cs`, `RenderBatch.cs`

- [ ] **D01** Procedural water mesh: `WaterMeshJob.cs` memperbarui vertices permukaan air berdasarkan `waterLevel` tiap sel — @Alvin — L — [P1]
- [ ] **D02** Water shader: material air yang responsif terhadap pollution (warna berubah dari biru→keruh saat polusi tinggi) — @Alvin — M — [P1]
- [ ] **D03** Heatmap overlay — absorption layer: render overlay warna di atas terrain berdasarkan nilai `absorptionRate` tiap sel — @Alvin — L — [P1]
- [ ] **D04** Heatmap overlay — elevation layer: render overlay warna berdasarkan tinggi elevasi terrain — @Alvin — M — [P1]
- [ ] **D05** Heatmap toggle: tombol UI untuk on/off heatmap dan switch antar layer — @Alvin + @Fajari — S — [P1]
- [ ] **D06** Heatmap legend: panel legend warna di sudut layar saat heatmap aktif — @Alvin — S — [P1]
- [ ] **D07** Critical zone visual alert: pulse effect (ring animasi merah) pada sel yang `waterLevel` melewati threshold kritis — @Alvin — M — [P2]
- [ ] **D08** Heatmap — risk layer: overlay risiko banjir gabungan (elevasi rendah + absorption rendah + waterLevel) — @Alvin — M — [P2]
- [ ] **D09** Texture Atlas: konsolidasi material bangunan ke satu atlas untuk mengurangi GPU state changes — @Alvin — L — [P2]

---

### MODULE E — Game Loop & State Management
> Owner utama: **@Fajari** · File terkait: `GameManager.cs`, `CityGameManager.cs`

- [ ] **E01** State machine utama: enum `GamePhase` (PLANNING, BUILDING, SIMULATING, HARVESTING) dengan transisi yang jelas — @Fajari — M — [P1]
- [ ] **E02** Phase transition — Planning → Simulation: tombol "Mulai Simulasi" memicu perubahan state dan menonaktifkan brush — @Fajari — S — [P1]
- [ ] **E03** Phase transition — Simulation → Harvest: setelah durasi simulasi selesai (atau trigger hujan berhenti), transisi otomatis ke harvest — @Fajari — S — [P1]
- [ ] **E04** Phase transition — Harvest → Planning: tombol "Lanjut" di harvest screen memulai siklus baru, reset simulasi — @Fajari — S — [P1]
- [ ] **E05** Siklus counter: track siklus 1-4, tampilkan di HUD, dan naikan kompleksitas cuaca per siklus — @Fajari — S — [P1]
- [ ] **E06** Weather intensity progression: curah hujan naik per siklus (Siklus 1: ringan → Siklus 4: ekstrem) — @Fajari + @Syukri — M — [P1]
- [ ] **E07** Income calculation: hitung pendapatan per siklus dari komposisi zona (industri tinggi, pertanian menengah, dll) — @Fajari — M — [P1]
- [ ] **E08** Economic penalty: jika zona pemukiman tergenang, kurangi income siklus berikutnya — @Fajari — M — [P1]
- [ ] **E09** End game trigger: setelah siklus 4 selesai, transisi ke Final Report screen — @Fajari — S — [P1]
- [ ] **E10** Pause system: pause/resume simulasi tanpa merusak Job state — @Fajari — M — [P2]
- [ ] **E11** Save/Load state: simpan `GamePhase`, `VoxelWorld`, `budget`, `cycleNumber`, `reputasi`, `publicEducation` ke file — @Fajari — L — [P2]

---

### MODULE F — Policy Card System
> Owner utama: **@Fajari** · Konten: **@Syukri + @Hamdi** · File: baru dibuat

- [ ] **F01** Buat `PolicyCard.cs` ScriptableObject: field `cardName`, `category` (Modern/Kearifan Lokal), `cost`, `effectType`, `effectValue`, `targetZone`, `insightText` — @Fajari — M — [P1]
- [ ] **F02** Buat `PolicyCardManager.cs`: kelola deck kartu yang tersedia per siklus, handle aktivasi — @Fajari — M — [P1]
- [ ] **F03** Implementasi efek Global Modifier: saat kartu diaktifkan, iterasi semua sel `targetZone` dan modifikasi nilai atributnya — @Fajari — M — [P1]
- [ ] **F04** Implementasi efek Local (sel terpilih): kartu yang butuh pemilihan sel (Kolam Retensi, Lubuk Larangan, Terasering) — @Fajari — L — [P1]
- [ ] **F05** Implementasi efek Temporal: kartu Sasi (menonaktifkan ekspansi industri 1 siklus, reset otomatis) — @Fajari — M — [P1]
- [ ] **F06** Prasyarat validasi: block aktivasi kartu jika kondisi tidak terpenuhi (Subak: butuh 3+ pertanian bersebelahan) — @Fajari — M — [P1]
- [ ] **F07** Buat 12 asset ScriptableObject kartu (data dari dokumen `dewan_bengawan_kartu_kebijakan.md`) — @Hamdi — M — [P1/P2]
- [ ] **F08** Budget deduction saat aktivasi kartu — @Fajari — XS — [P1]
- [ ] **F09** Unlock progression: define kartu apa yang tersedia di siklus 1/2/3/4 — @Fajari — S — [P1]
- [ ] **F10** Modulasi efektivitas berdasarkan `publicEducation` dan `reputation` (formula di design doc) — @Fajari — M — [P2]

---

### MODULE G — Reputation & Social-Technical System (Game Logic)
> Owner utama: **@Fajari** · Desain Formula: **@Syukri**

- [ ] **G01** Buat `SocialTechManager.cs`: track dua nilai float `reputation` (0-100) dan `publicEducation` (0-100) — @Fajari — S — [P2]
- [ ] **G02** Implementasi formula modulasi: `effectMultiplier = 0.5 + (0.3 × publicEducation/100) + (0.2 × reputation/100)` — @Fajari — S — [P2]
- [ ] **G03** Hook kartu ke `SocialTechManager`: kartu Sosialisasi Mitigasi menaikkan `publicEducation`, Pajak Beton menurunkan `reputation` — @Fajari — M — [P2]
- [ ] **G04** Tampilkan efektivitas aktual di tooltip kartu: "Efek: +10% absorption (efektivitas 70% — pendidikan publik rendah)" — @Alvin + @Fajari — M — [P2]
- [ ] **G05** Update panel UI 2-axis: render posisi pemain di kuadrant Reputasi × Pendidikan Publik — @Alvin — M — [P2]

---

### MODULE H — UI/UX Layer
> Owner utama: **@Alvin** · File: baru dibuat (UI prefabs & scripts)

- [ ] **H01** Main HUD top bar: budget indicator (ikon + angka monospace), cycle progress dots, weather forecast icon — @Alvin — M — [P1]
- [ ] **H02** Brush palette panel (bottom left): 5 slot zona + eraser, icon + label + warna, highlight slot aktif — @Alvin — M — [P1]
- [ ] **H03** Policy deck panel (bottom right): grid kartu yang unlock di siklus ini, filter Modern/Kearifan Lokal — @Alvin — L — [P1]
- [ ] **H04** Policy card detail modal: hover/klik kartu → modal detail (deskripsi, efek, cost, tombol Aktifkan) — @Alvin — M — [P1]
- [ ] **H05** Insight Card modal: full-screen takeover setelah simulasi, narasi edukatif + tombol Lanjut — @Alvin — M — [P1]
- [ ] **H06** Harvest screen: rapor mini (pendapatan, polusi, reputasi update), tombol Lanjut ke siklus berikutnya — @Alvin — M — [P1]
- [ ] **H07** Final Report screen: grafik 4 siklus (bar chart resilience, ekonomi, polusi), skor total — @Alvin — L — [P2]
- [ ] **H08** Reflection screen: 3 prompt refleksi tertulis (input text) + tombol submit — @Alvin — M — [P2]
- [ ] **H09** Map export: screenshot peta akhir + simpan ke file — @Alvin — M — [P2]
- [ ] **H10** Toast notification system: notifikasi kecil bottom-center untuk konfirmasi aksi (zona dicat, kartu aktif) — @Alvin — S — [P2]
- [ ] **H11** Reputasi 2-axis panel (mini): tampil di HUD setelah fitur G-series selesai — @Alvin — S — [P2]
- [ ] **H12** Main menu & splash screen — @Alvin — M — [P2]
- [ ] **H13** Settings panel: audio, grafis, kontrol — @Alvin — S — [P2]
- [ ] **H14** Pause menu in-game — @Alvin — S — [P2]

---

### MODULE I — Tutorial & Onboarding
> Owner utama: **@Alvin** · Konten: **@Syukri**

- [ ] **I01** Tutorial Step 1 — Brush: spotlight brush palette + instruksi cara melukis zona — @Alvin — M — [P2]
- [ ] **I02** Tutorial Step 2 — Zone Types: pop-up saat pertama kali hover tiap zona, menjelaskan atributnya — @Alvin — M — [P2]
- [ ] **I03** Tutorial Step 3 — Simulasi: instruksi cara klik "Mulai Simulasi" dan cara baca hasilnya — @Alvin — S — [P2]
- [ ] **I04** Tutorial Step 4 — Kartu Kebijakan: instruksi cara membuka deck dan mengaktifkan kartu — @Alvin — S — [P2]
- [ ] **I05** Briefing opening: screen pengantar peran sebagai "anggota dewan tata ruang" sebelum masuk gameplay — @Alvin + @Syukri — M — [P2]
- [ ] **I06** Kearifan lokal first-encounter pop-up: saat pertama kali kartu kearifan lokal tersedia, tampilkan mini-explainer budaya — @Alvin — S — [P2]
- [ ] **I07** Skip tutorial option: untuk sesi ulang — @Alvin — XS — [P2]

---

### MODULE J — Content & Educational Materials
> Owner utama: **@Syukri** · Pendukung: **@Hamdi**

- [ ] **J01** Finalisasi tabel parameter hidrologis tiap zona: nilai `absorptionRate` dan `flowFriction` berbasis literatur — @Syukri — M — [P1]
- [ ] **J02** Tulis narasi Insight Card untuk Siklus 1 (tema: tutupan lahan & resapan) — @Syukri — S — [P1]
- [ ] **J03** Tulis narasi Insight Card untuk Siklus 2 (tema: kausalitas keputusan & banjir) — @Syukri — S — [P1]
- [ ] **J04** Tulis narasi Insight Card untuk Siklus 3 (tema: kearifan lokal & efisiensi) — @Syukri — S — [P1]
- [ ] **J05** Tulis narasi Insight Card untuk Siklus 4 (tema: dimensi sosio-teknis kebijakan) — @Syukri — S — [P1]
- [ ] **J06** Lengkapi data ScriptableObject 12 kartu: salin dari `dewan_bengawan_kartu_kebijakan.md`, masukkan ke Unity asset — @Hamdi — M — [P1]
- [ ] **J07** Validasi akurasi hidrologis simulasi: bandingkan perilaku air di game dengan ekspektasi teoritis (sel beton harus lebih cepat tergenang dari sawah) — @Syukri — M — [P1]
- [ ] **J08** Susun skenario stress test hidrologi: 5 skenario (slope berbeda, intensitas berbeda) untuk benchmark akurasi — @Hamdi — M — [P1]
- [ ] **J09** Buat Glossary in-game: definisi 10-15 istilah kunci (koefisien limpasan, infiltrasi, CA, dll) — @Syukri — M — [P2]
- [ ] **J10** Rancang 2 skenario tugas pre/post-test untuk pilot (wilayah fiktif berbeda dari gim) — @Hamdi — L — [P2]
- [ ] **J11** Buat lembar validasi ahli (rubrik penilaian ahli materi + ahli media) — @Syukri — M — [P2]
- [ ] **J12** Susun lesson plan 60 menit untuk guru pendamping pilot — @Hamdi — M — [P2]

---

### MODULE K — Testing & Performance
> Owner utama: **@Fajari + @Hamdi** · Visual Test: **@Alvin**

- [ ] **K01** Unit test: validasi aturan CA (sel kedap air TIDAK menyerap air, sel hijau MENYERAP lebih banyak) — @Fajari — M — [P1]
- [ ] **K02** Integration test: satu siklus penuh berjalan tanpa crash pada grid 64³ — @Fajari — M — [P1]
- [ ] **K03** Latency test: ukur response time input zona ke visual feedback (target <100ms) — @Fajari — S — [P1]
- [ ] **K04** FPS benchmark di 3 spec laptop: high-end, mid-range, low-end (laptop sekolah) — @Fajari — M — [P1]
- [ ] **K05** Memory leak check: jalankan profiler Unity 10 menit non-stop, verifikasi memory tidak naik terus — @Fajari — S — [P1]
- [ ] **K06** Auto-tiling/connectivity check: pastikan zona yang berdampingan menghasilkan prosedural mesh yang koheren — @Alvin — M — [P2]
- [ ] **K07** SUS pre-test: 3-5 orang (tim luar) pakai gim dan isi SUS form sebelum pilot resmi — @Hamdi — S — [P2]
- [ ] **K08** Pilot test execution: 1 kelas SMA A (data collection, observasi, post-test) — @Hamdi + @Syukri — XL — [P2]
- [ ] **K09** Bug triage pasca-pilot: categorize dan prioritaskan bug dari pilot untuk difix sebelum CP5 — @Fajari — M — [P2]
- [ ] **K10** Pilot test 2: 1 kelas SMA B dengan build v2 post-iterasi — @Hamdi + @Syukri — XL — [P2]

---

### MODULE L — Documentation & Delivery
> Owner utama: **@Syukri** · Konten: semua anggota

- [ ] **L01** Setup GitHub repo struktur: folder Assets, Docs, Builds, Scripts terpisah rapi — @Syukri — S — [P1]
- [ ] **L02** README.md: deskripsi singkat proyek, cara build & run, kontributor — @Syukri — S — [P1]
- [ ] **L03** Weekly dev log: catatan 1 paragraf per minggu apa yang dikerjakan (bahan video karya akhir) — @Syukri — XS/minggu — [P1]
- [ ] **L04** Screenshot milestone: screengrab prototype setiap CP, simpan dengan label versi — @Syukri — XS/CP — [P1]
- [ ] **L05** Video pengembangan (proposal): produksi sesuai storyboard di `dewan_bengawan_storyboard_video.md` — @Alvin + Tim — XL — [P1]
- [ ] **L06** Persiapan dokumen HKI: kumpulkan source code sample + deskripsi karya untuk sentra HKI — @Hamdi — M — [P1]
- [ ] **L07** Rekam video diary iterasi: rekam perubahan besar pasca-pilot untuk bahan video karya akhir — @Syukri — S/iterasi — [P2]
- [ ] **L08** Video karya akhir (final stage): produksi video final 100% fungsional + cuplikan pilot — @Alvin + Tim — XL — [P2]
- [ ] **L09** Laporan akhir LIDM: kompilasi semua deliverable, hasil validasi, dan analisis data pilot — @Syukri + @Hamdi — XL — [P2]

---

## 4. Checkpoints

Setiap CP harus mendapat **sign-off dari Fajari (Ketua)** sebelum lanjut. Jika gagal gate, lakukan mini-sprint 3-5 hari dan re-evaluate.

---

### CP1 — Foundation Lock (Target: Akhir Minggu 2)
**Objective**: Infrastruktur kolaborasi dan fondasi simulasi siap.

| Kriteria Go/No-Go | PIC Verifikasi |
|---|---|
| ✅ Git repository setup, semua anggota bisa push & pull | Syukri |
| ✅ Unity Project terbuka di minimal 3 mesin tanpa error | Fajari |
| ✅ `WaterSimulationJob.cs` menjalankan simulasi air sederhana (1 siklus CA pada grid kecil 16³) | Fajari |
| ✅ `ZoneController.cs` bisa mengubah `ZoneType` sel melalui mouse click | Fajari |
| ✅ Nilai `absorptionRate` berubah otomatis saat zona diubah | Fajari + Syukri |
| ✅ HKI sudah didaftarkan ke sentra kampus (ada nomor pengajuan) | Hamdi |
| ✅ Minimal 1 SMA merespons positif email pilot | Hamdi |

---

### CP2 — Core Loop Demo-able (Target: Akhir Minggu 6)
**Objective**: 1 siklus penuh dapat dimainkan end-to-end.

| Kriteria Go/No-Go | PIC Verifikasi |
|---|---|
| ✅ Pemain bisa cat zona dengan brush tool (5 tipe zona) | Fajari |
| ✅ Bangunan muncul otomatis saat zona dicat | Alvin |
| ✅ Simulasi air berjalan ≥30 FPS pada grid 64×16×64 di laptop mid-range | Fajari |
| ✅ Heatmap absorption berfungsi dan bisa toggle | Alvin |
| ✅ 3+ kartu kebijakan dapat diaktifkan dan mengubah nilai atribut sel | Fajari |
| ✅ Harvest screen menampilkan income dan polusi siklus | Alvin |
| ✅ 1 Insight Card muncul setelah simulasi dengan konten yang benar | Syukri |
| ✅ Budget terpotong saat zonasi dan aktivasi kartu | Fajari |

---

### CP3 — Proposal Video Cut (Target: Akhir Minggu 8)
**Objective**: Build cukup stabil dan lengkap untuk direkam jadi video pengembangan LIDM (≥50% fungsional).

| Kriteria Go/No-Go | PIC Verifikasi |
|---|---|
| ✅ Semua 5 tipe zona berfungsi dengan parameter hidrologis yang benar | Syukri |
| ✅ Minimal 6 kartu kebijakan fungsional (termasuk 2 kearifan lokal) | Fajari |
| ✅ Dual heatmap (absorption + elevation) berfungsi | Alvin |
| ✅ Loop 4 siklus dapat diselesaikan tanpa crash | Fajari |
| ✅ Insight Card 4 konten (1/siklus) siap dengan narasi final | Syukri |
| ✅ Demo dapat direkam mulus (tidak ada bug visual kritis) | Fajari + Alvin |
| ✅ **VIDEO PENGEMBANGAN SELESAI DIPRODUKSI DAN DIUPLOAD KE YOUTUBE** | Alvin |
| ✅ **PROPOSAL LIDM TERSUBMIT** | Syukri + Fajari |

---

### CP4 — Feature Complete (Target: Akhir Minggu 12)
**Objective**: Semua fitur P2 selesai, build siap untuk pilot test di sekolah.

| Kriteria Go/No-Go | PIC Verifikasi |
|---|---|
| ✅ Semua 12 kartu kebijakan fungsional | Fajari |
| ✅ Sistem reputasi × pendidikan publik aktif dan terlihat efeknya | Fajari + Syukri |
| ✅ Final report, reflection, dan export screen berfungsi | Alvin |
| ✅ Tutorial 4-step selesai | Alvin |
| ✅ FPS ≥30 pada laptop low-end (spec lab komputer SMA) | Fajari |
| ✅ Skenario pre/post-test pilot siap | Syukri + Hamdi |
| ✅ Minimal 2 SMA mitra sudah konfirmasi tanggal pilot | Hamdi |
| ✅ Instrumen SUS dan rubrik berpikir spasial-kritis sudah tervalidasi ahli | Syukri |
| ✅ Hero assets 3-5 model Indonesia terintegrasi | Alvin |
| ✅ Artikel ilmiah versi draft selesai | Syukri |

---

### CP5 — Pilot-Ready & Final Build (Target: Akhir Minggu 14)
**Objective**: Build pasca-iterasi pilot siap untuk tahap final LIDM.

| Kriteria Go/No-Go | PIC Verifikasi |
|---|---|
| ✅ Data pilot dari 2 sekolah (≥60 siswa) terkumpul | Hamdi + Syukri |
| ✅ Analisis pre-post dan SUS selesai | Syukri |
| ✅ Bug kritis dari pilot sudah difix (build v3) | Fajari |
| ✅ Laporan akhir LIDM siap | Syukri |
| ✅ Artikel ilmiah tersubmit ke jurnal Sinta 3-4 | Syukri |
| ✅ Nomor HKI atau bukti pendaftaran tersedia | Hamdi |
| ✅ Semua deliverable LIDM final sudah terkirim | Fajari + Tim |

---

## 5. Team Assignment Matrix

| Modul | Fajari | Alvin | Syukri | Hamdi | Dosbing |
|---|---|---|---|---|---|
| A Core Simulation | **R/A** | R | C | — | — |
| B Zone & World | **R/A** | R | C | C | — |
| C Procedural Gen | C | **R/A** | — | — | — |
| D Rendering | C | **R/A** | C | — | — |
| E Game Loop | **R/A** | R | C | — | — |
| F Policy Cards | **R/A** | R | C | R | — |
| G Reputation | **R/A** | R | C | — | — |
| H UI/UX | C | **R/A** | — | — | — |
| I Tutorial | C | **R/A** | R | — | — |
| J Content | — | C | **R/A** | R | — |
| K Testing | **R/A** | R | R | **R** | — |
| L Docs & Delivery | A | R | **R/A** | R | C |

**R** = Responsible (eksekusi) · **A** = Accountable (owner, final quality) · **C** = Consulted (masukan)

---

## 6. Definition of Done

Task dinyatakan selesai **hanya jika** memenuhi semua kriteria berikut:
1. **Fungsional**: fitur berjalan sesuai acceptance criteria tanpa workaround.
2. **Terintegrasi**: kode di-merge ke branch `main` tanpa conflict.
3. **Tidak merusak**: semua task lain yang sebelumnya jalan tetap jalan.
4. **Terdokumentasi**: ada komentar inline minimal di fungsi utama.
5. **Direview**: minimal 1 anggota lain melihat hasilnya (async via screenshot/screen share/PR).

---

## 7. Ground Rules Tim

1. **No silent breakage**: jika Anda menggabungkan (merge) kode dan merusak modul lain, Anda wajib memperbaikinya segera.
2. **Weekly sync 1 jam**: Setiap awal minggu untuk melaporkan kemajuan, mendiskusikan blocker, dan merencanakan langkah selanjutnya.
3. **Kill switch**: Fajari selaku ketua tim berhak mengaktifkan *kill switch* pada fitur opsional jika mendekati tenggat waktu CP.
4. **Feature freeze**: 1 minggu sebelum setiap CP, tidak ada penambahan fitur baru — fokus pada perbaikan bug dan pemolesan (*polishing*).
