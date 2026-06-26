# Write-up: Branch `feat-Main-Menu` vs `develop`

> Dokumen ringkas perubahan branch `feat-Main-Menu`. Fokus: membangun **scene Main Menu**
> terpisah sesuai `docs/ui_spec.md §2` dan `docs/figma/Main Menu.png`. Ditulis sebagai acuan
> tim & basis PR ke `develop`.

Tanggal: 2026-06-26 · Branch: `feat-Main-Menu` · Base: `develop`

---

## 1. Ringkasan

Sebelumnya game langsung masuk ke scene gameplay (`_GAME_SYSTEM`) tanpa layar pembuka.
Branch ini menambahkan **scene `MainMenu` mandiri** (uGUI / Canvas) sebagai titik masuk game:
judul, latar dunia voxel, dan tiga menu **MULAI / PENGATURAN / KELUAR** dengan highlight yang
merespons **mouse hover** maupun **navigasi keyboard**.

| Kategori | Isi |
|---|---|
| **Scene** | `MainMenu.unity` baru (Canvas overlay, EventSystem Input System, kamera) |
| **Skrip** | `MainMenuController` (navigasi + aksi tombol), `MenuItem` (view highlight) |
| **Aset** | Font Times New Roman (Bold dipakai), 4 sprite UI (bg, title, holder, rail) |
| **Config** | Build Settings: `MainMenu` = index 0, `_GAME_SYSTEM` = index 1 |

---

## 2. File BARU

### Scene
- **`Assets/MainMenu/MainMenu.unity`** — scene pembuka. Hierarki:
  - `MainMenu_Canvas` (Screen Space - Overlay, `CanvasScaler` ScaleWithScreenSize 1920×1080)
    - `Background` — `Image` full-stretch (`bg_mainmenu`).
    - `Title` — `Image` pojok kanan-atas, jaga aspek (`title`, memuat "Dewan Bengawan").
    - `Rail` — `Image` garis pinggir bertitik di kiri (`menu_rail`).
    - `Item_MULAI` / `Item_PENGATURAN` / `Item_KELUAR` — masing-masing `Button` + `MenuItem`,
      berisi `Holder` (kapsul kuning `menu_holder`, default disembunyikan) + `Label` (teks).
    - Komponen `MainMenuController` menempel di Canvas.
  - `EventSystem` — pakai **`InputSystemUIInputModule`** (wajib; project memakai Input System baru,
    modul lama `StandaloneInputModule` akan error).
  - `Main Camera`.

### Skrip (`Assets/MainMenu/`)
- **`MainMenuController.cs`** — orkestrasi menu:
  - **Highlight terpusat**: hanya **satu** item aktif; default **tidak ada** yang aktif.
  - **Mouse**: hover → item aktif; keluar → mati. Hover juga mereset state keyboard
    (`keyboardIndex = -1`) supaya keluar-hover kembali ke *none*, tidak "bolak-balik" ke
    pilihan keyboard sebelumnya.
  - **Keyboard**: `↑`/`W` & `↓`/`S` memindah item aktif; `Enter`/`Space` menjalankan item aktif.
  - **Aksi**: `OnMulai()` → `SceneManager.LoadScene("_GAME_SYSTEM")`; `OnPengaturan()` → placeholder
    (log saja, menyusul); `OnKeluar()` → quit (`EditorApplication.isPlaying=false` di editor).
- **`MenuItem.cs`** — murni *view* satu item: `SetActive(bool)` menyalakan kapsul + mengubah warna
  teks (**hitam** saat aktif, **putih `#D9D9D9`** saat non-aktif). Melaporkan hover ke controller.

### Aset
- **`Assets/HUD/Fonts/timesbd.ttf`** — Times New Roman **Bold** (font menu, sesuai spec desain).
  *(`times/timesbi/timesi` ikut ter-copy namun belum dipakai — bisa dihapus bila ingin ramping.)*
- **`Assets/MainMenu/Sprites/`** — `bg_mainmenu`, `title`, `menu_holder`, `menu_rail`
  (copy dari `docs/figma/`, di-import sebagai **Sprite**).
- **`docs/figma/`** — sumber desain: `background Main Menu.png`, `Title.png`, `holder text menu.png`,
  `Garis pinggir Title.png`.

---

## 3. File yang DIMODIFIKASI

### `ProjectSettings/EditorBuildSettings.asset`
- Scene build diganti menjadi **`MainMenu` (index 0)** + **`_GAME_SYSTEM` (index 1)**.
  Sebelumnya hanya `SampleScene`. MULAI memuat scene index 1 by name.

---

## 4. Keputusan teknis penting

- **Scene terpisah, bukan overlay di scene game** — Main Menu sebagai scene sendiri = state bersih,
  transisi via `SceneManager.LoadScene`, tidak mengotori `_GAME_SYSTEM`.
- **uGUI (Canvas)** — konsisten dengan HUD gameplay & keputusan tim (`ui_spec.md §7`).
- **Highlight digerakkan controller, bukan `EventSystem.firstSelectedGameObject`** — versi awal
  memakai `firstSelectedGameObject` sehingga MULAI **selalu** aktif (bug). Diganti ke state terpusat
  di `MainMenuController` agar default *none* dan satu sumber kebenaran untuk mouse + keyboard.
- **Navigasi keyboard manual (baca `Keyboard.current`)** — tidak bergantung pada binding "Navigate"
  bawaan, jadi `WASD` + panah pasti jalan tanpa setup InputActions tambahan.
- **Legacy `Text` + Times New Roman** — konsisten dengan HUD (yang juga legacy `Text`); menu sengaja
  pakai Times (brand judul), berbeda dari HUD yang pakai Gabarito.

---

## 5. Cara menjalankan / testing

- Buka `Assets/MainMenu/MainMenu.unity`, masuk Play mode (atau Play dari Build Settings → mulai di index 0).
- **Default**: tiga item putih, tanpa kapsul (tidak ada yang aktif).
- **Mouse**: arahkan ke item → kapsul kuning + teks hitam; klik MULAI → masuk scene gameplay. ✅ teruji.
- **Keyboard**: `↓`/`S`, `↑`/`W` memindah highlight; `Enter`/`Space` menjalankan.
- **Mouse + keyboard**: hover mengambil alih; keluar-hover kembali ke *none* (tidak balik ke pilihan keyboard).

---

## 6. TODO ke depan

### Prioritas dekat
- [ ] **Posisi & spacing** sesuai mockup (rel, jarak antar item, lebar kapsul) — penyesuaian halus di Inspector.
- [ ] **Lebar kapsul adaptif** — sekarang lebar tetap (muat "PENGATURAN"); buat menyusut ngepas tiap teks.
- [ ] **Layar PENGATURAN** — saat ini hanya placeholder (log). Butuh desain + scene/panel + isi
  (volume, resolusi, bahasa, dll).

### Polish
- [ ] **Bullet `‣`** di depan item non-aktif sesuai mockup (opsional).
- [ ] **Transisi antar-scene** (fade) saat MULAI agar tidak terasa "jump".
- [ ] **Audio** — musik latar menu + SFX hover/klik.
- [ ] **Tombol KELUAR**: dialog konfirmasi sebelum quit (opsional).
- [ ] Rapikan font: hapus `times/timesbi/timesi` bila hanya Bold yang dipakai.

### Catatan kebersihan repo
- [ ] **`Assets/Screenshots/`** sudah menumpuk banyak PNG ter-track (dari beberapa sesi). Pertimbangkan
  `.gitignore` folder ini agar artefak debug tidak masuk repo ke depannya.

---

## 7. Catatan untuk reviewer / merge

- Scene baru `MainMenu.unity` berdiri sendiri — tidak menyentuh `_GAME_SYSTEM` (hanya dirujuk by name).
- Satu-satunya perubahan config: `EditorBuildSettings.asset` (urutan/daftar scene).
- File auto-generated tidak perlu di-review: `*.meta`.
- Belum ada test assembly; verifikasi manual di Editor (default state, hover, keyboard, MULAI→scene game).
