# 🎸 Guitar Exercise View — Milestone 1

> **Unity 2022.3 LTS** · URP · iOS-first · Procedural Fretboard

This document walks you through verifying Milestone 1: a procedurally generated fretboard with 6 animated strings, correct camera framing, and proper scene scaffolding.

---

## 1. Requirements

| Requirement | Version / Notes |
|---|---|
| **Unity Hub** | 3.x or later |
| **Unity Editor** | **2022.3.20f1** (exact version recommended) |
| **iOS Build Support** | Module installed via Unity Hub → Installs → Add modules |
| **macOS** (for iOS build) | Required only to build/deploy to device |
| **Git** | For cloning the repo |

> ⚠️ Using a different Unity version may cause import warnings. The project targets 2022.3.20f1.

---

## 2. Clone the Repository

```bash
git clone <your-repo-url> guitar-exercise-view
cd guitar-exercise-view
```

---

## 3. Open in Unity Hub

1. Launch **Unity Hub**.
2. Click **Open → Add project from disk**.
3. Navigate to the cloned `guitar-exercise-view/` folder (the one containing `Assets/` and `ProjectSettings/`).
4. Select the folder and click **Open**.
5. Confirm the editor version is **2022.3.20f1** in the version selector.

---

## 4. Wait for Initial Import

Unity will import all assets and compile scripts. This takes **1–3 minutes** on first open.

Watch the bottom status bar for:
- `Refreshing assets…`
- `Compiling scripts…`
- `Importing assets…`

When the progress bar disappears and the **Console** shows no errors, you are ready.

> **Common import warning:** "URP asset not found" — this is resolved in the next step when you run the setup menu.

---

## 5. Create the Milestone 1 Scene

1. In the Unity menu bar, click:
   ```
   GuitarExercise → Setup → Create Milestone 1 Scene
   ```
2. If prompted to save the current scene, choose **Don't Save** (or save if you've made changes).
3. Watch the **Console** window. You should see:
   ```
   ✅ Milestone 1 scene created. Press Play to verify.
      Scene saved to: Assets/Scenes/Milestone1.unity
   ```

The Scene Hierarchy should now contain:
```
FretboardRoot
  └── StringsRoot
CameraRig
KeyLight
FillLight
Background
```

---

## 6. Press Play

Click the **▶ Play** button in the Unity toolbar.

The **Game View** will activate and execute the runtime code:
- `FretboardMesh.Awake()` procedurally generates the fretboard mesh.
- `StringRenderer.Awake()` creates 6 LineRenderer child objects.
- `CameraRig.Awake()` positions the camera and logs visible world units to the Console.

---

## 7. What You Should See

### Game View
- A **dark background** (#1A1A2E charcoal blue) fills the screen.
- A **tapered wooden fretboard** occupies the center — wider at the body end (bottom of screen in portrait, right in landscape), narrower at the nut.
- **22 fret wires** as thin metallic ridges crossing the fretboard at equal-temperament spacing (frets get closer together toward the body).
- **Inlay dots** at frets 3, 5, 7, 9 (single pearl-white circles) and fret 12 (double dots).
- **6 guitar strings** as thin lines running the full scale length:
  - Strings 0–2 (e, B, G): metallic silver (#C0C0C0), thin
  - Strings 3–5 (D, A, E): warm bronze (#B8A878), progressively thicker
- Lighting: warm key light from upper-left, soft warm fill from the right.

### Scene View (Editor)
- Green wireframe box gizmo around the fretboard bounds.
- Yellow lines at each fret position.
- Camera frustum gizmo (blue lines) when `CameraRig` is selected.

### Console Output
```
[CameraRig] ─── Camera Info ───────────────────────────────
  Position       : (0.0, 1.8, 2.4)
  Rotation       : (338.0, 0.0, 0.0)
  FOV            : 55°
  Aspect Ratio   : 1.778  (16:9)
  Dist to Board  : ~4.8 units
  Visible Width  : ~7.6 units  (fretboard body width: 0.45)
  Visible Height : ~4.3 units
```

> **Visible Width should be well above 0.45 units** — confirming all 6 strings are within frame.

---

## 8. How to Tweak the Camera in Inspector

1. **Exit Play mode** (click ▶ again).
2. Select **CameraRig** in the Hierarchy.
3. In the **Inspector**, expand the `CameraRig` component.
4. Adjust:
   - `Camera Position` — move camera closer/farther/up/down
   - `Camera Rotation Euler` — change pitch (X) to look more steeply down
   - `Field Of View` — wider (higher value) to see more of the fretboard
5. **Right-click** the `CameraRig` component header → **Apply Camera Settings** to apply immediately without entering Play mode.
6. Enable **Live Preview** checkbox for real-time updates as you drag sliders.

Suggested adjustments for different view angles:

| View | Position | Rotation | FOV |
|---|---|---|---|
| Default (angled) | (0, 1.8, 2.4) | (-22, 0, 0) | 55 |
| Top-down | (0, 3.0, 6.0) | (-45, 0, 0) | 60 |
| Player POV | (0, 0.3, -1.0) | (5, 0, 0) | 70 |
| Wide shot | (0, 2.5, 3.0) | (-28, 0, 0) | 65 |

---

## 9. Exit Criteria Checklist

Use this checklist to confirm Milestone 1 is complete before proceeding to Milestone 2.

### Scene Structure
- [ ] `FretboardRoot` exists at world origin
- [ ] `FretboardMesh` component is attached with `numFrets=22`, `scaleLength=12`, `nutWidth=0.38`, `bodyWidth=0.45`
- [ ] `StringsRoot` is a child of `FretboardRoot` with `StringRenderer` component
- [ ] `CameraRig` exists with `Camera` (tagged `MainCamera`) and `CameraRig` component
- [ ] `KeyLight` (Directional, intensity 1.2) exists
- [ ] `FillLight` (Directional, warm, intensity 0.4) exists
- [ ] `Background` quad exists at (0, 0, 8) with dark material

### Visual — Fretboard
- [ ] Fretboard mesh renders with correct taper (narrower at nut end)
- [ ] 22 fret wires are visible as thin raised lines
- [ ] Fret spacing narrows toward the body end (equal-temperament scaling)
- [ ] Inlay dots visible at frets 3, 5, 7, 9 (single) and 12 (double)
- [ ] Fretboard has a slight concave radius (16" equivalent curve)

### Visual — Strings
- [ ] 6 strings span the full scale length
- [ ] Plain strings (e, B, G) appear silver/bright
- [ ] Wound strings (D, A, E) appear slightly warm/bronze tinted
- [ ] String thickness increases from high e to low E

### Camera
- [ ] FOV = 55°
- [ ] Position = (0, 1.8, 2.4)
- [ ] Rotation pitch = -22°
- [ ] Console shows "Visible Width" well above 0.45 (full board visible)
- [ ] All 6 strings are visible in Game View

### Code Quality
- [ ] No compiler errors in Console
- [ ] No NullReferenceException on Play
- [ ] `RegenerateMesh()` can be called from Inspector via right-click without errors
- [ ] `GuitarExercise/Verify/Log Camera Info` menu item works

### Build Settings (optional but recommended)
- [ ] File → Build Settings → Platform: iOS
- [ ] Company: BryanKaranja15, Product: GuitarExerciseView
- [ ] Color Space: Linear (Edit → Project Settings → Player → Other Settings)
- [ ] Scripting Backend: IL2CPP (iOS only)
- [ ] Target iOS Version: 15.0

---

## Project Structure

```
guitar-exercise-view/
├── Assets/
│   ├── Materials/
│   │   └── BackgroundMaterial.mat        (created by setup script)
│   ├── Scenes/
│   │   └── Milestone1.unity              (created by setup script)
│   ├── Scripts/
│   │   ├── Camera/
│   │   │   └── CameraRig.cs
│   │   ├── Editor/
│   │   │   └── MilestoneOneSetup.cs      (editor-only, not in build)
│   │   └── Fretboard/
│   │       ├── FretboardMesh.cs
│   │       └── StringRenderer.cs
│   └── Settings/
│       └── URPAsset.asset
├── Packages/
│   └── manifest.json
├── ProjectSettings/
│   ├── GraphicsSettings.asset
│   ├── InputManager.asset
│   ├── ProjectSettings.asset
│   ├── ProjectVersion.txt
│   ├── QualitySettings.asset
│   └── TagManager.asset
├── .gitignore
└── README_MILESTONE1.md                  ← you are here
```

---

## Troubleshooting

### "The type or namespace 'GuitarExerciseView' could not be found"
- Ensure all `.cs` files are inside `Assets/Scripts/` subfolders.
- Check that `.meta` files exist for each `.cs` file.
- Try: **Assets → Reimport All**

### "URP Asset not assigned"
- Go to **Edit → Project Settings → Graphics**.
- Drag `Assets/Settings/URPAsset.asset` into the **Scriptable Render Pipeline Settings** slot.

### Fretboard mesh not appearing
- Select `FretboardRoot` → right-click `FretboardMesh` component → **Regenerate Mesh**.
- Ensure a material is assigned (or it will use Unity's default pink error material).

### Strings not visible
- Check that `StringsRoot` is a child of `FretboardRoot`.
- Select `StringsRoot`, check `StringRenderer` component is enabled.
- In Scene View, confirm strings are at Y ≈ 0.012 (just above board surface).

### Camera shows wrong angle
- Select `CameraRig` → right-click `CameraRig` component → **Apply Camera Settings**.
- If `livePreview` is ON, the camera continuously resets to Inspector values during editor Update.

---

*Milestone 1 complete — fretboard rendered, strings visible, camera framed. Next: Milestone 2 (note markers, tablature data model, tap input).*
