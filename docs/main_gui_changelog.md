# Write-up: Branch `feat/Main-GUI` vs `main`

> Dokumen ringkas perubahan dari titik awal (`main`) sampai kondisi sekarang di branch
> `feat/Main-GUI`. Fokus: membangun **Main GUI / HUD** dari desain Figma, **data model**
> status kota, serta **sistem hujan** (cuaca → air). Ditulis sebagai acuan tim & basis PR.

Tanggal: 2026-06-26 · Branch: `feat/Main-GUI` · Base: `main`

---

## 1. Ringkasan

`main` belum punya HUD/UI sama sekali — game digerakkan murni keyboard/mouse (lihat CLAUDE.md
"Runtime controls"). Branch ini menambahkan **HUD lengkap berbasis uGUI**, **single source of
truth** untuk status kota (`EconomyManager`), dan **fitur hujan** yang menaikkan air secara
bertahap berikut visual partikelnya.

Statistik diff (kode + dokumen): **16 file, +897 / −11 baris.**

| Kategori | Isi |
|---|---|
| **Data model** | `EconomyManager` (Budget/Education/Trust/Weather) sbg sumber kebenaran + observer event |
| **HUD** | Top bar, toolbar kanan (brush + overlay picker), Policy Card stack, minimap, tombol fase |
| **Simulasi** | Hujan dari atas (menggenang) dipicu cuaca + visual Particle System |
| **Gaya** | Font Gabarito, palette `#ECE6DA`, rounded corners, separator, opacity bar |
| **Dokumen** | `CLAUDE.md`, `docs/ui_spec.md`, aset Figma, write-up ini |

---

## 2. File BARU

### Skrip — Data & orkestrasi
- **`Assets/EconomyManager/EconomyManager.cs`** — sumber kebenaran status kota. Field privat
  (budget/education/trust/weather), akses **read-only** via properti, mutasi **wajib** lewat
  method (`TrySpend`, `AddBudget`, `SetEducation`, `SetTrust`, `SetWeather`) yang memicu event
  `OnStatsChanged`. Ada `OnValidate` (`#if UNITY_EDITOR`) agar edit nilai di Inspector saat play
  ikut memicu event (kemudahan testing).

### Skrip — HUD (`Assets/HUD/`)
- **`HudController.cs`** — mengisi 4 label top bar; **berlangganan** `OnStatsChanged` (observer,
  bukan polling).
- **`BrushPicker.cs` + `BrushButton.cs`** — toolbar brush dengan menu **mekar saat hover**
  (fan-out). Ikon induk berubah jadi brush terpilih. Termasuk tombol **Kursor** (mode tanpa brush).
- **`OverlayPicker.cs` + `OverlayButton.cs` + `OverlayController.cs`** — picker overlay
  (Risiko/Resapan/Elevasi + tombol mata = kembali ke tanpa-overlay). **State sudah jalan;
  rendering heatmap masih TODO.**
- **`PolicyCardStack.cs`** — tumpukan kartu kebijakan; default menumpuk sejajar, **saat hover**
  naik + menyebar seperti mengipas (animasi lerp bebas frame-rate).
- **`MinimapController.cs`** — memindai `VoxelWorld` dari atas → `Texture2D` (warna per `zoneType`),
  refresh ter-throttle. Read-only.
- **`PhaseButton.cs`** — tombol "Mulai Simulasi" (kanan-bawah) → `GameManager.NextPhase()`.
  **Label adaptif** mengikuti fase berikutnya.
- **`RainVisual.cs`** — mengatur intensitas Particle System hujan dari `EconomyManager.Weather`
  (Cerah=mati, Hujan/Badai=emisi). Murni kosmetik.

### Aset
- **`Assets/HUD/Fonts/Gabarito-Variable.ttf`** — font brand (variable, lisensi OFL, dari Google Fonts).
- **`docs/figma/*.svg`** — aset desain Figma (gambar 4–22, dll).
- **TextMesh Pro/** — TMP Essentials ter-import (TMP **belum dipakai**; HUD pakai legacy `Text` +
  Gabarito — lihat keputusan §5).
- **`CLAUDE.md`, `docs/ui_spec.md`** — panduan repo & spesifikasi mapping Figma→kode.

---

## 3. File yang DIMODIFIKASI

### `Assets/GameManager/GameManager.cs`
- Mengaktifkan referensi `public EconomyManager economyManager;` (sebelumnya dikomentari).
- `NextPhase()` diubah `private` → **`public`** (agar bisa dipanggil tombol HUD; Enter tetap jalan).
- `Update()` memanggil `economyManager.SystemUpdate(world)` tiap frame (placeholder).

### `Assets/ZoneController/ZoneController.cs`
- **Menghapus** field `currentBudget` lokal; anggaran kini **single source of truth** di
  `EconomyManager`. Melukis memanggil `economyManager.TrySpend(costPerBlock)` (gagal jika dana kurang).
- Enum `PaintTool` ditambah `None`; method `SetActiveZone(byte)` & `SetCursorMode()` untuk dipanggil
  tombol UI (menggantikan peran tombol digit 0–5).

### `Assets/WaterSimulation/WaterSimulationSystem.cs`
- Tambah referensi `EconomyManager` + parameter hujan.
- **`UpdateRain()`** — tiap tick, sejumlah kolom acak ditetesi air **tepat di atas permukaan**;
  CA mengalirkannya → **menggenang & level naik perlahan**. Intensitas: Cerah=0, Hujan=6, Badai=18
  tetes/tick (semua tunable). Pola mengikuti `RiseFloodLogic` yang sudah ada. Spasi (injeksi instan)
  tetap dipertahankan untuk testing.

### `Assets/GameManager/_GAME_SYSTEM.unity` (scene)
- GameObject baru: `EconomyManager`, `OverlayController`, `HUD_Canvas` (+ seluruh hierarki HUD),
  `RainSystem` (Particle System di-parent ke kamera). Wiring antar-komponen via Inspector.

---

## 4. Perjalanan fitur (urutan pengerjaan)

1. **Analisis Figma** → `docs/ui_spec.md` (mapping desain ke kode, keputusan, open questions).
2. **Data model** `EconomyManager` + migrasi anggaran dari `ZoneController` (single source of truth).
3. **Top bar** (4 segmen: Anggaran/Berpendidikan/Kepercayaan/Cuaca) + `HudController` (observer live).
4. **Toolbar kanan** — brush picker (fan-out hover) lalu overlay picker.
5. **Policy Card stack** (animasi tumpuk → mengipas saat hover).
6. **Minimap** "PETA KOTA" (read-only dari dunia).
7. **Tombol "Mulai Simulasi"** (label adaptif).
8. **Polish** — font Gabarito, palette `#ECE6DA`, rounded corners, opacity bar 0.85, separator top bar.
9. **Logika data top bar** — diverifikasi live (budget/edu/trust/cuaca berubah otomatis).
10. **Cuaca → Air** — `UpdateRain` (hujan menggenang) + visual Particle System.
11. **Iterasi visual hujan** — follow kamera tanpa lag (World + Inherit Velocity 0.6), opacity 50%,
    sudut **terkunci 45° kiri-bawah** (Billboard + start rotation, bukan stretch-by-velocity).

---

## 5. Keputusan teknis penting

- **uGUI, bukan UI Toolkit** — lebih ramah pemula & cocok untuk tim.
- **Legacy `Text` + Gabarito, bukan TMP** — TMP Essentials ada tapi tidak dipakai; migrasi TMP =
  churn besar (`Text`→`TMP_Text` + font asset) tanpa nilai signifikan untuk MVP. **Direkomendasikan
  tetap legacy** sampai ada masalah ketajaman teks.
- **Observer pattern** (`OnStatsChanged`) — HUD & visual hujan bereaksi pada event, bukan polling.
  Konsekuensi: ubah nilai langsung di Inspector tak memicu update → ditangani `OnValidate` (editor-only).
- **Konvensi subsistem** — semua modul di-update lewat `SystemUpdate(VoxelWorld)`/`OnUpdate()` yang
  dipanggil `GameManager`, bukan `Update()` sendiri.
- **Logika air = kustom, bukan asset eksternal** — asset air (Crest/Aquas/Obi) tak nyambung dengan
  voxel CA. Asset eksternal hanya untuk visual (di sini cukup Particle System bawaan).
- **Hujan: World space + Inherit Velocity + Billboard rotasi tetap** — kombinasi ini memberi
  "best of both worlds": ikut kamera (anti-lag), tetap terasa nyata (parallax), sudut terkunci 45°.

---

## 6. Cara menjalankan / testing

- Buka scene `Assets/GameManager/_GAME_SYSTEM.unity`, masuk Play mode.
- **Planning**: mouse melukis zona; tombol brush picker memilih zona; tombol Kursor menonaktifkan brush.
  Anggaran berkurang via `TrySpend` dan top bar ikut berubah.
- **Tombol "Mulai Simulasi"** (kanan-bawah) atau `Enter` memajukan fase.
- **Cuaca**: ubah field `weather` di `EconomyManager` (Inspector) saat play → top bar & hujan langsung
  berubah. Hujan menaikkan air bertahap selama fase **Simulation**.
- Catatan dev: saat mengendalikan Editor via tooling tanpa fokus Game view, `Update()` & partikel
  nyaris tak ber-tick; verifikasi pakai forced step (lihat memory `mcp-playmode-no-tick`).

---

## 7. TODO ke depan

### Prioritas dekat (melengkapi loop)
- [ ] **Sumber pengubah cuaca in-game** — sekarang cuaca hanya bisa diubah via Inspector
  (`OnValidate` editor-only). Perlu sistem: progres waktu / event / Policy Card → `SetWeather()`.
- [ ] **Overlay heatmap rendering** — state Risiko/Resapan/Elevasi sudah jalan, visualisasinya belum.
- [ ] **Logika Education & Trust** — masih placeholder; belum ada aturan yang mengubah nilainya.
- [ ] **Policy Card: muka kartu** (judul/efek/biaya) + aksi klik & logika kebijakan (menunggu desain).

### Polish UI (ditunda, dari `ui_spec.md` §7b)
- [ ] **Diskrepansi brush vs keyboard** — tombol `1` (Rumput/`GRASS`) & `2` (Beton/`CONCRETE`) belum
  ada di brush picker (dibahas tim; butuh ikon bila ditambah).
- [ ] Brush picker: posisi/2-baris sesuai Figma + highlight brush aktif.
- [ ] **Minimap**: kotak viewport kamera (proyeksi frustum).
- [ ] **Upgrade ke TextMeshPro** — *direkomendasikan skip untuk MVP* (lihat §5).
- [ ] Tuning sudut/densitas hujan & rasio Inherit Velocity bila perlu.

### Modul besar belum dibangun
- [ ] **Harvest screen** (fase Panen).
- [ ] **Menu Pengaturan** (belum ada desain & kode).
- [ ] Layar Main Menu (bila masuk scope).

---

## 8. Catatan untuk reviewer / merge

- Perubahan **scene** (`_GAME_SYSTEM.unity`) besar — review hierarki HUD_Canvas & RainSystem.
- File auto-generated tidak perlu di-review: `*.meta` baru, `Packages/packages-lock.json`,
  `FloodSimulator.slnx`, isi `Assets/TextMesh Pro/` (import bawaan).
- Aset Figma `.svg` hanya referensi (URP belum punya importer SVG; analisis lewat PNG).
- Tidak ada test assembly; verifikasi dilakukan manual di Editor + bukti numerik/visual.
