# Pupil Neon XR — Calibration App

A Unity 6 / MRTK3 calibration application for the **Pupil Labs Neon** eye tracker running on **Meta Quest 3**. It provides two hand‑driven (MRTK3 pinch) calibration flows and exports calibration data as JSON.

> **This repository is a fork of [Pupil Labs' MRTK3 fork](https://github.com/pupil-labs)** (their Neon‑integrated fork of the Microsoft Mixed Reality Toolkit 3 dev template). All of the calibration‑specific work lives under `Assets/CalibrationApp/`; the rest of the tree is the upstream MRTK3 + Pupil Labs Neon SDK that this app depends on.

---

## What it does

- **Main scene** — a bilingual (EN/JP) instructions panel, then a navigation menu to launch either calibration flow.
- **Recording Frustum Calibration** — choose a recording mode (Left Eye / Right Eye / Binocular), shows 13 fixed‑angle markers at 2 m, a head‑stability ring, and exports a session JSON.
- **Sensor Offset Calibration** — *(stub — see [Known limitations](#known-limitations))*.

All interaction uses **MRTK3 pinch** via `PressableButton.OnClicked` (no XRI ray interactors). Every UI surface is a World Space Canvas with a `TrackedDeviceGraphicRaycaster`.

---

## Requirements

- **Unity 6000.3.16f1** (the version this project was authored in; other Unity 6 patch releases should work).
- The **MRTK3** packages — **vendored in this repo** under `Packages/` (no extra setup).
- The **Pupil Labs Neon SDK** — **not redistributed here**; you must add it yourself (see [Pupil Labs SDK — required separately](#pupil-labs-sdk--required-separately)).
- A **Meta Quest 3** with the Pupil Labs Neon attached, for on‑device use.

---

## Important folders (calibration app)

Everything authored for this app is under **`Assets/CalibrationApp/`**:

| Folder | Contents |
|---|---|
| `Scenes/` | `Main.unity` (startup), `CalibRecordingFrustum.unity`, `CalibSensorOffset.unity` |
| `Scripts/` | Runtime logic — `MainMenuController`, `ModeSelector`, `MarkerSpawner`, `StabilityIndicator`, `ExportController`, `CalibrationMarkers`, `FaceCamera`, `ProceduralTorus` |
| `Scripts/Editor/` | Editor utilities — `JapaneseFontSetup.cs`, `RepoSetup.cs` |
| `Prefabs/` | `MarkerDot.prefab` (sphere + billboarded TMP ID label) |
| `Materials/` | `MarkerBright.mat`, `StabilityRing.mat` |
| `Fonts/` | `NotoSansJP-VariableFont_wght.ttf`, `JP Dynamic SDF.asset`, `OFL.txt` |

These depend on assets **outside** `CalibrationApp/`:
- `Packages/org.mixedrealitytoolkit.*` — the 14 MRTK3 packages, **vendored (embedded)** into this repo so it opens standalone, without the original MRTK monorepo. BSD 3-Clause; license notices retained.
- `Assets/PupilLabs/` — the Pupil Labs Neon integration, including the `MRTK NeonXR Variant`, `PL MRTK XR Rig Variant`, and `PL MRTKInputSimulator Variant` rig prefabs the scenes are built on. **Not included in this repository** (see below).
- `Assets/Plugins/` — Neon native libraries (part of the Pupil Labs integration).
- `Packages/manifest.json` also pulls `com.pupil-labs.neon-xr.core` from Pupil Labs' git repo at build time.

### Pupil Labs SDK — required separately

The Pupil Labs Neon content ships **without an explicit license**, so it is **not redistributed in this repository**: `Assets/PupilLabs/` is git-ignored, and `Assets/Plugins/` (Neon native libs) should be treated the same way. A fresh clone will therefore be **missing the Neon rig prefabs**, and the calibration scenes will show missing references until you add them.

To run the app after cloning:
1. Obtain the Pupil Labs Neon Unity integration from Pupil Labs (their MRTK3 fork / `neon-xr` project) and copy its `Assets/PupilLabs/` (and Neon `Assets/Plugins/`) into this project.
2. Reopen the scenes — the rig references will resolve.

`com.pupil-labs.neon-xr.core` is already wired as a git dependency in `manifest.json`, so the core gaze scripts download automatically; only the in-`Assets` content must be added manually.

---

## Scenes & build settings

The three calibration scenes are registered in **Build Settings** with `Main` as the startup scene:

1. `Assets/CalibrationApp/Scenes/Main.unity`
2. `Assets/CalibrationApp/Scenes/CalibSensorOffset.unity`
3. `Assets/CalibrationApp/Scenes/CalibRecordingFrustum.unity`

Flow: **Main** shows the instructions canvas (dismiss with the pinch button) → the navigation canvas appears → its two buttons `SceneManager.LoadScene` into the calibration scenes.

---

## Recording Frustum Calibration — details

- **Modes:** `LeftEye` (default), `RightEye`, `Binocular`, chosen with three toggle buttons; the active one is highlighted.
- **Markers:** 13 dots placed at fixed `(azimuth, elevation)` offsets, 2.0 m from the recording camera:

  ```
  (1, 0,0) (2,15,0) (3,-15,0) (4,0,15) (5,0,-15)
  (6,25,0) (7,-25,0) (8,0,25) (9,0,-25)
  (10,18,18) (11,-18,18) (12,18,-18) (13,-18,-18)
  ```

  Placement: `worldPos = camPos + (camRot * Quaternion.Euler(-el, az, 0) * Vector3.forward) * 2`. Angles are relative to the **selected recording camera**, not centre eye.
- **Recording camera position per mode:**
  - `Binocular` → `Camera.main` position.
  - `LeftEye` / `RightEye` → queried via `InputTracking.GetNodeStates()` for `XRNode.LeftEye` / `RightEye`, converted to world space through the Camera Offset.
  - **IPD fallback (mandatory):** if the runtime eye node returns zero, the position falls back to `centre ± right * 0.03175 m` (Quest 3 fixed IPD ÷ 2). On Quest 3 the X offset is reliable but Y/Z often collapse to centre eye, so this fallback is required, not optional.
- **Stability ring:** a torus at the bottom of view; its colour lerps **green → red** as head angular speed goes from **1.5 to 6.0 deg/s**. Hold still until it is green before recording.
- **Export:** the "Export Session JSON" button writes to `Application.persistentDataPath`:

  ```
  frustum_session_<yyyyMMdd_HHmmss>.json
  ```

  ```json
  {
    "recordingMode": "LeftEye",
    "markerDistance": 2.0,
    "markers": [ { "id": 1, "az": 0, "el": 0 }, ... ]
  }
  ```

  A confirmation label shows the filename after writing.

---

## Japanese font (read before publishing)

All UI text is bilingual (EN/JP). Japanese glyphs are supplied by **Noto Sans JP (SIL Open Font License)**, built into a dynamic TMP font asset (`Fonts/JP Dynamic SDF.asset`) and registered as a **global TMP fallback**, so every TMP element renders Japanese without per‑object font assignment.

- `Fonts/NotoSansJP-VariableFont_wght.ttf` **must stay** — the dynamic font asset references it at build time.
- `Fonts/OFL.txt` **must stay** — it is the font license and must ship with the repo.
- If the font ever needs rebuilding (e.g. after a clean clone), run **`Tools ▸ CalibrationApp ▸ Build and Register Japanese Font (Noto)`**. It uses a project `.ttf`/`.otf` if present, otherwise downloads Noto Sans JP from the official OFL source. It will **not** use a proprietary system font (e.g. Windows Yu Gothic), and the `.gitignore` blocks `*.ttc` so proprietary fonts can never be committed.

---

## Getting started

1. Add the Pupil Labs SDK content (see [Pupil Labs SDK — required separately](#pupil-labs-sdk--required-separately)) — required for the rig.
2. Open the project in **Unity 6000.3.16f1**; the vendored MRTK packages and git dependencies resolve automatically.
3. Open `Assets/CalibrationApp/Scenes/Main.unity`.
4. Press Play to test in‑editor with the MRTK Input Simulator, or build for **Android** (Quest 3) via *File ▸ Build Settings*.
5. (Optional) If Japanese shows as missing boxes, run the font menu item above.

---

## Editor utilities

Found under `Assets/CalibrationApp/Scripts/Editor/` (menu: **Tools ▸ CalibrationApp**):

- **Build and Register Japanese Font (Noto)** — rebuilds the JP font asset and registers the TMP fallback.
- **Write or Update .gitignore** — writes/merges a Unity `.gitignore` at the project root (already run). Safe to re‑run.

These are development conveniences; you may delete them before publishing if you prefer a leaner repo.

---

## Known limitations

- **`CalibSensorOffset` is a stub** — currently a clean duplicate of `Main`. The sensor‑offset flow and its `config.json` output are not yet implemented.
- Eye‑node positions on Quest 3 are unreliable in Y/Z; the hardcoded IPD fallback is used by design.

---

## Licensing & attribution

- **MRTK3** — **BSD 3-Clause** (Mixed Reality Toolkit Contributors). Vendored under `Packages/` with its license/notice files retained.
- **Pupil Labs Neon SDK** — the `neon-xr` repo and `Assets/PupilLabs` ship **without an explicit license** (all rights reserved by default), so they are **not redistributed here**. `Assets/PupilLabs/` is git-ignored; `neon-xr.core` is pulled from Pupil Labs' repo at build time. Internal research use is consistent with how Pupil Labs distributes the SDK, but confirm terms with Pupil Labs before redistributing any of their content.
- **Noto Sans JP** — SIL Open Font License; keep `Assets/CalibrationApp/Fonts/OFL.txt`.
- `MarkerBright.mat` / `StabilityRing.mat` are derived from Pupil Labs' calibration-point material; regenerate them from a neutral shader if you need them fully unencumbered.

Before publishing publicly: do **not** commit proprietary fonts (`.gitignore` guards `*.ttc`) or the Pupil Labs content (`.gitignore` excludes `Assets/PupilLabs`), and remove the `com.coplaydev.unity-mcp` dev dependency from `Packages/manifest.json` (it is an editor-automation tool, not part of the app).
