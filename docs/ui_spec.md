# UI Spec — Dewan Bengawan

Dokumen ini memetakan mockup Figma (`docs/figma/`) ke arsitektur kode yang sudah ada.
Tujuannya: jadi acuan bersama tim **sebelum** menyentuh Unity, terutama soal data model
(anggaran/pendidikan/kepercayaan/cuaca) yang **belum ada** di kode.

> Status sumber: semua aset adalah **raster** (PNG / SVG yang membungkus PNG). Belum ada
> nilai warna hex, font, atau spacing dari desainer — lihat bagian [Yang Masih Kurang](#yang-masih-kurang).

---

## 1. Inventaris aset Figma

| File | Isi visual | Peran di UI |
|------|------------|-------------|
| `Main Menu.png` | Layar menu utama | Scene **MainMenu** |
| `Main UI(5).png` | HUD gameplay penuh | Acuan layout **HUD** |
| `gambar 10.svg` | Minimap "PETA KOTA" 1:8 + legenda | Widget **Minimap** |
| `gambar 21.svg` | Kartu biru pola emas "KEBIJAKAN" | **Policy Card** (punggung kartu) |
| `gambar 5.svg` | Icon Anak SD | Top bar — tingkat pendidikan |
| `gambar 4.svg` | Uang "Rp" | Ikon **Anggaran** |
| `gambar 7.svg` | Dasi/jas merah | Ikon **Kepercayaan** (trust) |
| `gambar 9.svg` | Logo BMKG | Widget **Cuaca** |
| `gambar 11.svg` | Rumah+Gedung | Icon default tool brush (akan ekspansi ke kiri jika hover untuk memperlihatkan brush-brush). anggap sebagai placeholder untuk membuka pilihan brush |
| `gambar 12.svg` | Rumah | Brush Kategori **Hunian** |
| `gambar 13.svg` | Gedung/pabrik | Brush Kategori **Industri** |
| `gambar 17.svg` | Tetes air | Toggle overlay **Resapan** |
| `gambar 18.svg` | Segitiga seru | Toggle overlay **Bahaya/Risiko** |
| `gambar 14.svg` | Pohon | Brush Kategori **Hijau/Hutan** |
| `gambar 15.svg` | Mata | Mirip gambar 11, yaitu ketika hover membuka pilihan untuk memilih tipe overlay yang diinginkan **overlay** |
| `gambar 16.svg` | Panah ke atas | toggle overlay **elevasi** |
| `gambar 22.svg` | Tombol play | Tombol **Mulai Simulasi** |
| `holder text.svg` | Kapsul kuning | Background tombol/label (mis. "MULAI") |

---

## 2. Layar: Main Menu

Komponen: judul "Dewan Bengawan", tombol **MULAI**, **PENGATURAN**, **KELUAR**, latar dunia voxel.

| Aksi | Hubungan kode |
|------|---------------|
| MULAI | Load scene gameplay → `GameManager.Start()` (sudah otomatis `Initialization → Planning`) |
| PENGATURAN | **Belum ada desain & kode**, menyusul bukan prioritas utama untuk DL juli |
| KELUAR | `Application.Quit()` |

---

## 3. Layar: HUD Gameplay (`Main UI(5).png`)

### 3a. Top bar (status global)

Top bar dibagi **4 bagian** (kiri → kanan):

| # | Elemen | Ikon | Format | Data backing | Status kode |
|---|--------|------|--------|--------------|-------------|
| 1 | Anggaran (`Rp 1.428 jt`) | `gambar 4` | mata uang Rp | `EconomyManager.Budget` | ✅ data + HUD live (`HudController`, observer `OnStatsChanged`); `ZoneController.TrySpend` mengurangi & label auto-update. Teruji play mode. |
| 2 | Berpendidikan | `gambar 5` (avatar) | **persentase 0–100%** | `EconomyManager.Education` | ✅ HUD live. 🟡 logika pengubah nilai masih placeholder (belum ada aturan game). |
| 3 | Kepercayaan | `gambar 7` | **persentase 0–100%** | `EconomyManager.Trust` | ✅ HUD live. 🟡 logika pengubah masih placeholder. |
| 4 | Cuaca (`Badai`) | `gambar 9` (BMKG) | label/enum | `EconomyManager.Weather` | ✅ HUD live. ✅ **terkait ke air**: `WaterSimulationSystem.UpdateRain()` menurunkan air dari atas & menggenang (Hujan=6 tetes/tick, Badai=18), naik perlahan. Visual hujan: `RainVisual` + Particle System (parented ke kamera). Diuji play mode. |

> Catatan: `50/200` pada mockup adalah placeholder metrik **Berpendidikan** — diputuskan
> diganti ke format **persen**. Avatar (`gambar 5`) menempel di bagian Berpendidikan, bukan identitas pemain.

> **Blocker utama:** keempat elemen top bar butuh data model baru. Lihat
> [Data model yang harus dibuat](#5-data-model-yang-harus-dibuat).

### 3b. Toolbar kanan — palet bangunan & overlay

Toolbar terdiri dari **2 tombol induk** yang masing-masing **mekar (fan-out) saat hover**.
Ini menggantikan input keyboard digit `0–5` yang sekarang dipakai `ZoneController` untuk memilih voxel aktif.

**Tombol induk 1 — Brush picker (`gambar 11`, ikon rumah+gedung).** ✅ **Sudah dibangun**
(`Assets/HUD/BrushPicker.cs` + `BrushButton.cs`; pojok kanan-atas). Hover → mekar ke kiri
memunculkan pilihan; klik brush = `ZoneController.SetActiveZone(id)` + ikon induk berubah jadi brush terpilih:

Urutan kiri → kanan: **Industri · Hunian · Agricultural · Water_Green · Kursor**.

| Brush | Ikon (file) | Asal | zoneId | Efek di `ZoneController` |
|-------|------|------|--------|----------|
| Industri | `brush_industrial` | `gambar 13` | `31` | `SetActiveZone(31)` → `ZONE_INDUSTRIAL`, tool=Brush |
| Hunian | `brush_residential` | `gambar 12` | `30` | `SetActiveZone(30)` → `ZONE_RESIDENTIAL`, tool=Brush |
| Agricultural | `brush_agricultural` | `Agricultural.png` (padi) | `32` | `SetActiveZone(32)` → `ZONE_AGRICULTURAL`, tool=Brush |
| Water_Green | `brush_watergreen` | `gambar 14` (pohon) | `33` | `SetActiveZone(33)` → `ZONE_WATER_GREEN` (biopori/resapan), tool=Brush |
| Kursor (tanpa brush) | `brush_cursor` | `cursor.png` | `-1` | `SetCursorMode()` → tool=None, melukis nonaktif |

Induk (`brush_palette`/`gambar 11`) hanya ikon awal; setelah memilih, **ikon induk berubah** jadi brush terpilih.
Game **mulai dalam mode Kursor** (tanpa brush) agar pemain tidak sengaja melukis.

> Catatan: brush "Hijau" (pohon) dipetakan ke `ZONE_AGRICULTURAL` (Tani) — bukan tipe hutan baru.
> `ZONE_WATER_GREEN` (33, biopori/resapan) **belum** dapat tombol di palet — ditunda, catatan ke depan.

**Tombol induk 2 — Overlay picker.** ✅ **Sudah dibangun** (`Assets/HUD/OverlayPicker.cs`
+ `OverlayButton.cs` + `OverlayController.cs`; Baris 2, di bawah brush picker). Pola fan-out sama.
Ikon **mata (`gambar 15`)** = tombol kembali ke *tanpa overlay* (ekuivalen Kursor di brush) + ikon induk default.
Klik → `OverlayController.SetOverlay(mode)`. **State sudah jalan; rendering heatmap-nya yang belum.**

| Overlay | Ikon | mode | Visualisasi (TODO render) | Field sumber |
|---------|------|------|-------------|--------------|
| Bahaya / Risiko | `gambar 18` | 1 | Heatmap risiko banjir | (turunan hasil simulasi) |
| Resapan | `gambar 17` | 2 | Daya serap tanah per sel | `VoxelCell.absorptionRate` |
| Elevasi | `gambar 16` | 3 | Ketinggian voxel | (tinggi kolom solid per `x,z`) |
| Off (tanpa overlay) | `gambar 15` (mata) | 0 | — | `OverlayController.SetNone()` |

### 3c. Minimap (`gambar 10`) ✅ **Sudah dibangun**

- `Assets/HUD/MinimapController.cs`; panel kiri-bawah. `RawImage` menampilkan `Texture2D` 32×32.
- Sumber data: scan `VoxelWorld` per kolom (voxel permukaan), warnai per `zoneType`; refresh tiap 0.5s.
- Warna: Hunian=kuning, Industri=oranye, Tani=hijau, Resapan=teal, Air=biru, tanah=beige. Legenda 3 item.
- ❌ Belum ada **kotak viewport kamera** (perlu proyeksi frustum) — ditunda ke polish.

### 3d. Policy Card + Bottom Bar ✅ **Sudah dibangun (visual)**

- `Assets/HUD/PolicyCardStack.cs`; pojok kiri-bawah. 5 kartu (`policy_card` = `Icon Kartu Full.png`).
- **Default**: kartu **menumpuk sejajar**, hanya ~2/3 terlihat (sepertiga bawah tertutup bottom bar).
- **Hover**: **menyebar sedikit ke kanan + naik tipis + sedikit membesar** (lerp halus, `IPointerEnter/Exit`).
- **BottomBar** (`HUD_Canvas/BottomBar`): strip gelap full-width mirip top bar (tinggi 56) → layar berbingkai
  garis atas + bawah, sekaligus menutup 1/3 bawah kartu.
- ❌ Belum ada **muka kartu** (judul/efek/biaya), aksi klik, & logika kebijakan — menunggu desain.

### 3e. Tombol Mulai Simulasi (`gambar 22`) — ✅ dibangun

- Memicu `GameManager.NextPhase()` (setara tombol `Enter`): `Planning → Construction → Simulation → Harvest`.
- Implementasi: `Assets/HUD/PhaseButton.cs` di pojok kanan-bawah HUD_Canvas (`PhaseButton_Mulai`). `NextPhase()` diubah jadi `public`.
- **Label adaptif** mengikuti fase berikutnya: Planning→"Mulai Konstruksi", Construction→"Mulai Simulasi", Simulation→"Panen", Harvest→"Selesai".
- Polish nanti: ganti ikon/gaya sesuai `gambar 22`, palette & font Spec Gaya.

---

## 4. Pemetaan UI → GameState

| GameState | UI yang aktif |
|-----------|---------------|
| `Initialization` | (transisi otomatis, tanpa UI) |
| `Planning` | Toolbar palet + minimap + top bar; tombol "Mulai Simulasi" untuk lanjut |
| `Construction` | Coroutine `CityGameManager`; UI progres *(tak ada mockup)* |
| `Simulation` | Overlay air/risiko aktif; top bar cuaca relevan |
| `Harvest` | **Layar Harvest — tak ada mockup sama sekali** |

---

## 5. Data model

✅ **Sudah dibuat:** `EconomyManager` (`Assets/EconomyManager/EconomyManager.cs`), MonoBehaviour
ber-`SystemUpdate(VoxelWorld)` mengikuti konvensi subsistem (dipanggil `GameManager`, bukan self-update).
Ter-wiring di GameObject `_GAME_SYSTEM` → child `EconomyManager`.

- `Budget` (long) — anggaran kota. **Sumber kebenaran tunggal**; `ZoneController` memotongnya via `TrySpend()`.
- `Education` (float 0–100) — tingkat pendidikan warga, persen. *(placeholder, belum ada logika)*
- `Trust` (float 0–100) — kepercayaan publik, persen; nanti dipengaruhi hasil simulasi banjir. *(placeholder)*
- `Weather` (enum: Cerah/Hujan/Badai) — nanti memodulasi laju injeksi air di `Simulation`. *(placeholder)*

Akses: properti read-only (`Budget`/`Education`/`Trust`/`Weather`); ubah hanya lewat
`TrySpend`/`AddBudget`/`SetEducation`/`SetTrust`/`SetWeather`. Event `OnStatsChanged` untuk update HUD.

---

## 6. Yang masih kurang (minta ke desainer)

1. **Layar Harvest/Panen** — modul wajib, tanpa mockup.
2. **Muka Policy Card** — layout isi kartu kebijakan.
3. **Layar Pengaturan** — ada di menu, tak ada desain.
4. **Legenda/skala Heatmap** — gradasi warna risiko & legendanya.
5. **State interaksi** — hover / selected / disabled, tooltip, panel detail bangunan.
6. **Spec gaya** — warna hex, font, ukuran, spacing (atau akses file Figma sumber).

## 7. Keputusan & pertanyaan terbuka

**Sudah diputuskan:**
- Top bar = 4 bagian: **Anggaran (Rp)** · **Berpendidikan (%)** · **Kepercayaan (%)** · **Cuaca (BMKG)**.
- Metrik Berpendidikan & Kepercayaan pakai **format persentase**.
- **Toolkit UI: uGUI (Canvas)** — dipilih demi beginner-friendly, tutorial melimpah, dukungan world-space (minimap/tooltip), dan friksi rendah untuk proyek Unity pertama tim.

**Masih perlu jawaban tim:**
1. Apakah `ZONE_WATER_GREEN` (biopori) dapat tombol palet sendiri? -> belum di buat, jadi kedepannya saja.

---

## 7b. TODO polish (ditunda)

- [ ] **Diskrepansi brush vs keyboard** — tombol keyboard `1` (Rumput/`GRASS`) & `2` (Beton/`CONCRETE`) **belum ada** di brush picker (picker hanya zona 30–33 + Kursor). Sedang dibahas tim: apakah material terrain dasar masuk picker yang sama? Kalau ya, perlu ikon Rumput & Beton (belum ada di aset Figma).
- [x] **Penyesuaian color palette HUD** — ✅ panel (top/bottom bar, flyout picker, minimap) = krem `#ECE6DA` (Spec Gaya §9), tombol putih untuk kontras, teks gelap `#1F2329`. Ikon/swatch/kartu dibiarkan apa adanya. Top & bottom bar alpha 0.85 (agak tembus). Tombol picker/CTA & panel minimap pakai sudut membulat (UISprite 9-slice, `pixelsPerUnitMultiplier=2.5`).
- [x] **Font Gabarito** — ✅ `Assets/HUD/Fonts/Gabarito-Variable.ttf` (variable, OFL, dari Google Fonts) dipakai semua label HUD. Ukuran: angka utama top bar & CTA = 28; judul minimap = 21; legenda mikro = 14 (21 kebesaran utk panel kecil).
- [ ] Brush picker: posisi/2-baris sesuai Figma, animasi mekar, highlight brush aktif.
- [ ] **Upgrade label ke TextMeshPro** — *ditunda & direkomendasikan SKIP utk MVP*: legacy `Text` + Gabarito sudah cukup crisp; migrasi TMP butuh ganti tipe `Text`→`TMP_Text` di `HudController`/`PhaseButton` + buat TMP Font Asset dari variable font + re-wiring (churn besar menjelang DL). Aktifkan hanya bila ada masalah ketajaman teks.
- [ ] Overlay picker (Baris 2): Risiko/Resapan/Elevasi (pola fan-out sama).

## 8. Rekomendasi urutan implementasi

1. Sepakati jawaban Bagian 7 + buat **data model** Bagian 5 (blocker).
2. Bangun **HUD top bar** + **toolbar palet** (menggantikan input digit di `ZoneController`).
3. **Minimap** (read-only dari `VoxelWorld`).
4. **Overlay air/risiko** (butuh sistem render baru).
5. **Policy Card** & **Harvest** (menunggu desain tambahan).

## 9 Spec Gaya
1. Warna Background elemen: #ECE6DA. 
2. font: Gabarito, 28 untuk elemen utama seperti angka. 21 untuk keterangan, seperti tambahan tulisan "kepercayaan", "berpendidikan".