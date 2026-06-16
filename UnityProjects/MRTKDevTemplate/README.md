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
- The **MRTK3** packages and **Pupil Labs Neon SDK** that ship in this fork (already present under `Packages/` and `Assets/PupilLabs/`).
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

These depend on assets **outside** `CalibrationApp/` and must remain in the project:
- `Assets/PupilLabs/` — the Neon SDK, the `MRTK NeonXR Variant`, `PL MRTK XR Rig Variant`, and `PL MRTKInputSimulator Variant` prefabs the scenes are built on.
- `Assets/Plugins/` — Neon native libraries.
- `Packages/` — MRTK3, XR Interaction Toolkit, TextMeshPro, Addressables.

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

1. Open the project in **Unity 6000.3.16f1**; let Package Manager restore dependencies.
2. Open `Assets/CalibrationApp/Scenes/Main.unity`.
3. Press Play to test in‑editor with the MRTK Input Simulator, or build for **Android** (Quest 3) via *File ▸ Build Settings*.
4. (Optional) If Japanese shows as missing boxes, run the font menu item above.

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

- **MRTK3** — MIT (Microsoft).
- **Pupil Labs Neon SDK / MRTK3 fork** — see Pupil Labs' license; this app is a fork of their work and inherits those terms.
- **Noto Sans JP** — SIL Open Font License; see `Assets/CalibrationApp/Fonts/OFL.txt`.
- `MarkerBright.mat` / `StabilityRing.mat` are derived from Pupil Labs' calibration point material.

Before publishing publicly, confirm you are complying with the Pupil Labs SDK license for any redistributed third‑party content, and do **not** commit proprietary fonts (the `.gitignore` already guards `*.ttc`).
