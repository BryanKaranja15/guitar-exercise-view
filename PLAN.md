# Guitar Exercise View — Unity + Swift Prototype Plan
**Version:** 0.1.0 | **Date:** 2026-05-22 | **Status:** Pre-Production

---

## Table of Contents

1. [Unity Project Structure](#1-unity-project-structure)
2. [3D Fretboard Model Spec](#2-3d-fretboard-model-spec)
3. [Guitar Skin System](#3-guitar-skin-system)
4. [Note Highway System](#4-note-highway-system)
5. [Tab/Song Data System](#5-tabsong-data-system)
6. [Timing Engine](#6-timing-engine)
7. [Input Pipeline](#7-input-pipeline)
8. [Hit Detection & Scoring](#8-hit-detection--scoring)
9. [Feedback System](#9-feedback-system)
10. [HUD](#10-hud)
11. [Red Rocks Background](#11-red-rocks-background)
12. [Unity ↔ Swift Bridge](#12-unity--swift-bridge)
13. [Prototype Milestones](#13-prototype-milestones)
14. [Technical Risks & Mitigations](#14-technical-risks--mitigations)

---

## 1. Unity Project Structure

### 1.1 Unity Version & Render Pipeline

- **Unity Version:** 6000.0.x LTS (Unity 6)
- **Render Pipeline:** Universal Render Pipeline (URP) 17.x
- **Color Space:** Linear
- **Target API:** Metal (iOS), Vulkan (Android)
- **Minimum iOS:** 16.0 | **Minimum Android API:** 26 (Oreo)
- **Scripting Backend:** IL2CPP | **.NET Standard:** 2.1

### 1.2 Folder Hierarchy

```
Assets/
├── _Project/                          # All project-specific code & assets
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GuitarExercise.asmdef
│   │   │   ├── TimingEngine.cs
│   │   │   ├── SongClock.cs
│   │   │   ├── ScoreManager.cs
│   │   │   └── GameStateMachine.cs
│   │   ├── Highway/
│   │   │   ├── GuitarExercise.Highway.asmdef
│   │   │   ├── NoteHighway.cs
│   │   │   ├── NoteView.cs
│   │   │   ├── NotePool.cs
│   │   │   ├── HitZoneMarker.cs
│   │   │   └── FretboardScroller.cs
│   │   ├── Input/
│   │   │   ├── GuitarExercise.Input.asmdef
│   │   │   ├── IInputProvider.cs
│   │   │   ├── KeyboardInputProvider.cs
│   │   │   ├── MicrophoneInputProvider.cs
│   │   │   ├── CoreMLPitchDetector.cs
│   │   │   ├── MIDIInputProvider.cs
│   │   │   └── InputRouter.cs
│   │   ├── Tabs/
│   │   │   ├── GuitarExercise.Tabs.asmdef
│   │   │   ├── SongData.cs
│   │   │   ├── NoteEvent.cs
│   │   │   ├── RandomTabGenerator.cs
│   │   │   └── TabParser.cs
│   │   ├── Guitar/
│   │   │   ├── GuitarExercise.Guitar.asmdef
│   │   │   ├── GuitarSkin.cs            (ScriptableObject)
│   │   │   ├── GuitarSkinApplicator.cs
│   │   │   └── FretboardMeshBuilder.cs
│   │   ├── Feedback/
│   │   │   ├── GuitarExercise.Feedback.asmdef
│   │   │   ├── HitFeedbackController.cs
│   │   │   ├── StringVibrationAnimator.cs
│   │   │   ├── CameraShakeController.cs
│   │   │   ├── ParticleHitEffect.cs
│   │   │   └── FloatingTextSpawner.cs
│   │   ├── HUD/
│   │   │   ├── GuitarExercise.HUD.asmdef
│   │   │   ├── HUDController.cs
│   │   │   ├── ScoreDisplay.cs
│   │   │   ├── MultiplierDisplay.cs
│   │   │   └── AccuracyBar.cs
│   │   ├── Background/
│   │   │   ├── GuitarExercise.Background.asmdef
│   │   │   ├── ParallaxBackground.cs
│   │   │   └── RedRocksSceneController.cs
│   │   └── Bridge/
│   │       ├── GuitarExercise.Bridge.asmdef
│   │       ├── NativeBridge.cs
│   │       └── BridgeMessageTypes.cs
│   ├── Scenes/
│   │   ├── GuitarExercise.unity          # Main gameplay scene
│   │   └── Bootstrap.unity               # Minimal init scene
│   ├── Prefabs/
│   │   ├── Highway/
│   │   │   ├── NoteCapsule.prefab
│   │   │   ├── HitZonePlane.prefab
│   │   │   └── StringMesh.prefab
│   │   ├── Feedback/
│   │   │   ├── HitParticleSystem.prefab
│   │   │   ├── MissParticleSystem.prefab
│   │   │   └── FloatingText.prefab
│   │   └── Guitar/
│   │       └── FretboardRoot.prefab
│   ├── ScriptableObjects/
│   │   ├── Skins/
│   │   │   ├── Skin_Default.asset
│   │   │   ├── Skin_Rosewood.asset
│   │   │   ├── Skin_Maple.asset
│   │   │   └── Skin_Ebony.asset
│   │   └── Songs/
│   │       └── (generated at runtime, not stored here)
│   ├── Art/
│   │   ├── Fretboard/
│   │   │   ├── T_Fretboard_Rosewood_Albedo.png   (2048×512)
│   │   │   ├── T_Fretboard_Rosewood_Normal.png
│   │   │   ├── T_Fretboard_Rosewood_Smoothness.png
│   │   │   ├── T_Fretboard_Maple_Albedo.png
│   │   │   ├── T_Fretboard_Ebony_Albedo.png
│   │   │   ├── T_Inlay_Dots.png               (2048×512, alpha mask)
│   │   │   ├── T_Inlay_Birds.png
│   │   │   └── T_Inlay_Blocks.png
│   │   ├── Strings/
│   │   │   ├── T_String_Steel.png             (64×64 tileable)
│   │   │   └── T_String_Gold.png
│   │   ├── Background/
│   │   │   ├── RedRocks_Layer0_Sky.png        (2560×720)
│   │   │   ├── RedRocks_Layer1_Rocks.png      (2560×720, PNG with alpha)
│   │   │   └── RedRocks_Layer2_Crowd.png      (2560×720, PNG with alpha)
│   │   ├── Notes/
│   │   │   └── T_NoteCapsule_Gradient.png     (128×64)
│   │   └── HUD/
│   │       └── (UI sprites)
│   ├── Materials/
│   │   ├── M_Fretboard_Rosewood.mat
│   │   ├── M_Fretboard_Maple.mat
│   │   ├── M_Fretboard_Ebony.mat
│   │   ├── M_String_Steel.mat
│   │   ├── M_NoteCapsule.mat
│   │   └── M_HitZone.mat
│   ├── Audio/
│   │   ├── SFX_Hit_Perfect.ogg
│   │   ├── SFX_Hit_Good.ogg
│   │   └── SFX_Miss.ogg
│   └── Shaders/
│       ├── S_FretboardPBR.shader
│       ├── S_StringVibration.shader
│       └── S_NoteGlow.shader
├── Plugins/
│   ├── iOS/
│   │   ├── NativeBridgePlugin.mm
│   │   └── CoreMLPitchBridge.mm
│   └── Android/
│       └── NativeBridgePlugin.java
└── StreamingAssets/
    └── CoreML/
        └── PitchDetector.mlpackage       (placeholder empty bundle)
```

### 1.3 Assembly Definitions

Each `*.asmdef` references only its required siblings to keep compile times fast:

| Assembly | References |
|---|---|
| `GuitarExercise` (Core) | *(none)* |
| `GuitarExercise.Tabs` | Core |
| `GuitarExercise.Guitar` | Core |
| `GuitarExercise.Highway` | Core, Tabs |
| `GuitarExercise.Input` | Core |
| `GuitarExercise.Feedback` | Core, Highway |
| `GuitarExercise.HUD` | Core |
| `GuitarExercise.Background` | Core |
| `GuitarExercise.Bridge` | Core, HUD |

### 1.4 Scene Hierarchy: `GuitarExercise.unity`

```
[Scene Root]
├── --- MANAGERS ---
│   ├── GameManager               (GameStateMachine.cs)
│   ├── TimingEngine              (TimingEngine.cs, SongClock.cs)
│   ├── ScoreManager              (ScoreManager.cs)
│   └── InputRouter               (InputRouter.cs)
├── --- CAMERA RIG ---
│   ├── CameraRoot                (empty, world position pivot)
│   │   ├── CameraArm             (local offset = [0, 1.8, 2.4])
│   │   │   └── MainCamera        (Camera component, FOV=55)
│   │   └── CameraShakeTarget     (CameraShakeController.cs)
├── --- GUITAR / HIGHWAY ---
│   ├── FretboardRoot             (FretboardMeshBuilder.cs, GuitarSkinApplicator.cs)
│   │   ├── FretboardBody         (MeshFilter + MeshRenderer, M_Fretboard_*)
│   │   ├── Strings               (6× StringMesh children)
│   │   ├── Inlays                (quad mesh, inlay texture — aesthetic only, no gameplay role)
│   │   └── HitZonePlane          (HitZoneMarker.cs)
│   └── NoteHighway               (NoteHighway.cs, NotePool.cs)
│       └── [NotePool – 64 pooled NoteCapsule instances]
├── --- BACKGROUND ---
│   └── BackgroundCanvas          (ParallaxBackground.cs, Screen Space – Camera)
│       ├── Layer0_Sky            (RawImage, depth=0)
│       ├── Layer1_Rocks          (RawImage, depth=1)
│       └── Layer2_Crowd          (RawImage, depth=2)
├── --- HUD ---
│   └── HUDCanvas                 (Screen Space – Overlay, HUDController.cs)
│       ├── ScoreLabel            (TextMeshProUGUI)
│       ├── MultiplierBadge       (MultiplierDisplay.cs)
│       ├── AccuracyBar           (AccuracyBar.cs, Slider component)
│       └── SettingsButton        (Button → NativeBridge.SendMessage)
├── --- FEEDBACK ---
│   ├── FloatingTextPool          (FloatingTextSpawner.cs)
│   ├── HitParticleRoot           (ParticleHitEffect.cs)
│   └── MissParticleRoot
└── --- BRIDGE ---
    └── NativeBridge              (NativeBridge.cs, DontDestroyOnLoad)
```

---

## 2. 3D Fretboard Model Spec

### 2.1 Geometry

The fretboard is a **procedurally generated mesh** built at startup by `FretboardMeshBuilder.cs`. No external DCC tool is required for the base mesh; art textures are layered on top via PBR materials.

**Fretboard dimensions (Unity units, 1 unit = ~1 metre for camera math):**

| Parameter | Value |
|---|---|
| Total length (Z axis) | 12.0 units |
| Width (X axis) at nut | 0.38 units |
| Width (X axis) at body join (12th fret) | 0.44 units (slight taper) |
| Thickness (Y axis) | 0.04 units |
| Number of frets modelled | 15 (frets 0–15, nut + 14 playable) |
| Fret spacing | Equal-tempered: `fretSpacing[n] = scaleLength / (2^(n/12))` with `scaleLength = 12.0` |

**Procedural mesh algorithm in `FretboardMeshBuilder.BuildMesh()`:**

```csharp
public static Mesh BuildFretboardMesh(int fretCount = 15)
{
    // Main board: single quad strip, 2 vertices per fret position × 2 edges
    // Vertices laid out left→right (low-fret → high-fret) along -Z → +Z
    // X width tapers linearly from NUT_WIDTH to BODY_WIDTH

    const float SCALE_LENGTH = 12.0f;
    const float NUT_WIDTH    = 0.38f;
    const float BODY_WIDTH   = 0.44f;
    const float THICKNESS    = 0.04f;

    float[] fretZ = new float[fretCount + 1];
    fretZ[0] = 0f; // nut at Z=0
    for (int i = 1; i <= fretCount; i++)
        fretZ[i] = SCALE_LENGTH - SCALE_LENGTH / Mathf.Pow(2f, i / 12f);

    // Top face: 2 * (fretCount+1) verts
    // Bottom face: same, offset by -THICKNESS on Y
    // Build UVs so U=0 at nut, U=1 at fret 15; V=0 at bass edge, V=1 at treble edge
    // Returns Mesh with sub-mesh 0 = top face, sub-mesh 1 = sides
}
```

**Fret wire geometry — Option C (selected):** No fret wire geometry is rendered. The fretboard surface uses wood texture and inlay overlays for aesthetics only (see §2.2 UV Layout and §3). All gameplay information is carried by the string lanes and the note pills that scroll along them; each note pill displays the fret number directly in its `FretLabel` child (see §4.2). Fret lines baked into the albedo texture serve as a visual reference only and have no collider or gameplay significance.

> **Design Decision — why not Option A (windowed fretboard)?**
> Option A (windowed fretboard) was rejected because held notes and bends span multiple fret positions — a windowed approach would clip sustains mid-scroll. Option C keeps fret lines as texture only.

### 2.2 UV Layout

- **Sub-mesh 0 (top face):** UV island occupies full 0→1 space. U runs nut→body, V runs bass E string → treble e string. Fretboard wood texture tiles at (1.0, 1.0). Inlay texture is a separate alpha-masked quad.
- **Sub-mesh 1 (sides/edges):** UV strip packed in bottom 10% of texture atlas (V=0.0→0.1).

### 2.3 String Meshes

Each of the 6 strings is an **independent procedural ribbon mesh** generated by `FretboardMeshBuilder.BuildStringMesh()`.

```csharp
// String positions (X = lateral, string 0 = low E, string 5 = high e)
// Y = just above fretboard surface, varies by open-string action
static readonly float[] STRING_X = { -0.165f, -0.099f, -0.033f, 0.033f, 0.099f, 0.165f };
static readonly float[] STRING_Y = { 0.008f,  0.007f,  0.006f,  0.005f, 0.005f, 0.004f };

// String gauges map to ribbon width (in Unity units):
// E=0.046" → 0.0028u, A=0.036" → 0.0022u, D=0.026" → 0.0016u
// G=0.017" → 0.0010u, B=0.013" → 0.0008u, e=0.010" → 0.0006u
static readonly float[] STRING_WIDTH = { 0.0028f, 0.0022f, 0.0016f, 0.0010f, 0.0008f, 0.0006f };
```

Each string mesh has:
- 2 vertices per fret segment × 15 fret segments = 30 vertices per string
- UV mapped for a tileable `T_String_Steel.png` (V tiles 8× along length)
- `M_String_Steel` uses a **custom `S_StringVibration.shader`** (see §9)

### 2.4 Camera Rig — Exact Values

```
CameraRoot world position:    (0.0,  0.0,  0.0)   ← pivot at fretboard center

CameraArm local position:     (0.0,  1.8,  2.4)   ← 1.8u above, 2.4u in front
CameraArm local rotation:     (-22°, 0°,   0°)     ← 22° downward tilt

MainCamera local position:    (0.0,  0.0,  0.0)   ← mounted at arm tip
MainCamera FOV:               55°
MainCamera Near Clip:         0.1
MainCamera Far Clip:          50.0
MainCamera Projection:        Perspective
```

**Result:** At these values, the fretboard occupies the bottom ~40% of the screen in 16:9 landscape. The nut (left of screen, fret 0) maps to approximately X=-0.22 NDC, and fret 15 maps to approximately X=+0.48 NDC. Strings appear as horizontal lanes separated by ~40px in a 1920×1080 render.

**Hit Zone screen position:** The `HitZonePlane` is placed at `Z = fretZ[2]` (≈1.37 units from nut) which projects to ~22% from the left edge of the screen at the above camera settings.

---

## 3. Guitar Skin System

### 3.1 GuitarSkin ScriptableObject

```csharp
// Assets/_Project/Scripts/Guitar/GuitarSkin.cs
[CreateAssetMenu(fileName = "Skin_New", menuName = "GuitarExercise/Guitar Skin")]
public class GuitarSkin : ScriptableObject
{
    [Header("Identity")]
    public string   skinId;           // e.g. "rosewood_dots_sunburst"
    public string   displayName;      // e.g. "Sunburst Rosewood"
    public Sprite   thumbnailSprite;  // 256×256 preview for shop UI

    [Header("Fretboard Wood")]
    public FretboardWoodType woodType;          // enum: Rosewood, Maple, Ebony
    public Texture2D fretboardAlbedo;           // 2048×512
    public Texture2D fretboardNormal;           // 2048×512
    public Texture2D fretboardSmoothnessAO;     // 2048×512 (R=smoothness, G=AO)
    [Range(0f, 1f)] public float fretboardSmoothnessMult = 0.72f;
    [Range(0f, 1f)] public float fretboardMetallic       = 0.0f;

    [Header("Inlay Pattern")]
    public InlayPatternType inlayType;          // enum: Dots, Birds, Blocks, None, Custom
    public Texture2D inlayAlbedo;               // 2048×512, alpha = inlay mask
    public Color     inlayTint = Color.white;

    [Header("Body / Finish")]
    public Color  bodyColorTop    = new Color(0.65f, 0.18f, 0.05f, 1f); // sunburst orange
    public Color  bodyColorBottom = new Color(0.05f, 0.02f, 0.01f, 1f); // sunburst black
    [Range(0f, 1f)] public float bodyMetallic   = 0.05f;
    [Range(0f, 1f)] public float bodyGloss      = 0.88f;

    [Header("Strings")]
    public StringMaterialType stringMaterial;   // enum: Steel, NickelWound, Gold, Nylon
    public Color              stringTint        = Color.white;
    public Texture2D          stringAlbedo;     // 64×64 tileable
    [Range(0.5f, 2.0f)] public float stringWidthMult = 1.0f;

    // Fret wire fields removed — Option C: no fret wire geometry; fret lines are texture-only.

    [Header("Note Capsule Tint (per-string)")]
    public Color[] stringNoteColors = new Color[6]
    {
        new Color(1.0f, 0.35f, 0.1f),  // E  – orange
        new Color(1.0f, 0.80f, 0.1f),  // A  – yellow
        new Color(0.2f, 0.85f, 0.3f),  // D  – green
        new Color(0.2f, 0.55f, 1.0f),  // G  – blue
        new Color(0.7f, 0.3f, 1.0f),   // B  – purple
        new Color(1.0f, 1.0f, 1.0f),   // e  – white
    };
}

public enum FretboardWoodType { Rosewood, Maple, Ebony }
public enum InlayPatternType  { Dots, Birds, Blocks, None, Custom }
public enum StringMaterialType { Steel, NickelWound, Gold, Nylon }
```

### 3.2 Material Slots on FretboardRoot

`GuitarSkinApplicator.cs` holds serialized references to all renderer components:

```csharp
public class GuitarSkinApplicator : MonoBehaviour
{
    [SerializeField] MeshRenderer fretboardRenderer;   // sub-mesh 0=top, 1=sides
    [SerializeField] MeshRenderer[] stringRenderers;   // [6] string ribbon renderers
    [SerializeField] MeshRenderer inlayRenderer;       // alpha-blended quad on top

    // MaterialPropertyBlock reuse (avoids material instance allocation)
    private MaterialPropertyBlock _mpb = new MaterialPropertyBlock();

    public void ApplySkin(GuitarSkin skin)
    {
        // --- Fretboard ---
        _mpb.SetTexture("_BaseMap",      skin.fretboardAlbedo);
        _mpb.SetTexture("_BumpMap",      skin.fretboardNormal);
        _mpb.SetTexture("_MetallicGlossMap", skin.fretboardSmoothnessAO);
        _mpb.SetFloat("_Metallic",       skin.fretboardMetallic);
        _mpb.SetFloat("_Smoothness",     skin.fretboardSmoothnessMult);
        fretboardRenderer.SetPropertyBlock(_mpb);

        // --- Inlay ---
        _mpb.Clear();
        _mpb.SetTexture("_BaseMap", skin.inlayAlbedo);
        _mpb.SetColor("_BaseColor",  skin.inlayTint);
        inlayRenderer.SetPropertyBlock(_mpb);

        // --- Strings ---
        for (int i = 0; i < 6; i++)
        {
            _mpb.Clear();
            _mpb.SetTexture("_BaseMap", skin.stringAlbedo);
            _mpb.SetColor("_BaseColor",  skin.stringTint);
            _mpb.SetFloat("_WidthMult",  skin.stringWidthMult);
            stringRenderers[i].SetPropertyBlock(_mpb);
        }

    }
}
```

### 3.3 Skin Swap Flow

1. `GuitarSkin` asset selected (from `Resources/Skins/` or Addressables key).
2. `GuitarSkinApplicator.ApplySkin(skin)` called — uses `MaterialPropertyBlock` throughout; zero new material instances created.
3. Skin selection persisted as `PlayerPrefs.SetString("ActiveSkinId", skin.skinId)`.
4. On scene load, `GuitarSkinApplicator.Awake()` calls `Resources.Load<GuitarSkin>("Skins/" + PlayerPrefs.GetString("ActiveSkinId", "Skin_Default"))`.

---

## 4. Note Highway System

### 4.1 NoteData Struct

```csharp
// Assets/_Project/Scripts/Highway/NoteView.cs (data half lives in Tabs assembly)
[System.Serializable]
public struct NoteData
{
    public int    stringIndex;     // 0=low E … 5=high e
    public int    fretNumber;      // 0=open, 1–15
    public double songTimeSeconds; // when note should be hit (in song clock time)
    public float  durationSeconds; // sustain length (0 for normal note, >0 for held note)
    public byte   velocity;        // 0–127 MIDI-style velocity
    public NoteType noteType;      // enum: Normal, Hammer, Pull, Bend, Slide
}

public enum NoteType { Normal, HammerOn, PullOff, Bend, Slide }
```

### 4.2 NoteView Prefab: `NoteCapsule.prefab`

```
NoteCapsule (root)
├── CapsuleMesh      (MeshFilter=CapsulePrimitive scaled [0.08, 0.035, 0.035], MeshRenderer=M_NoteCapsule)
├── FretLabel        (TextMeshPro 3D, font size 18pt, Z offset +0.036)
└── GlowHalo         (SpriteRenderer, T_NoteCapsule_Gradient, additive blend, scale=1.2)
```

**Note pill shape and fret number:** Every note is rendered as a pill (capsule). The `FretLabel` child displays the fret number so the player always knows what to fret — this is the sole carrier of fret-position information (no fret wire geometry exists; see §2.1). Normal (short) notes use the default capsule scale. **Held notes and bend notes are elongated pills:** their Z scale is set to `durationSeconds * SCROLL_SPEED` so the pill stretches to represent the full sustain duration visually. These elongated pills travel the complete length of the string lane scrolling at the same speed as all other notes — **there is no fret-position snapping** at any point during the scroll; the pill's X position is fixed to the string lane and its Z position is purely time-driven.

**Note colors** are looked up from `ActiveSkin.stringNoteColors[note.stringIndex]` and injected via `MaterialPropertyBlock` on `CapsuleMesh`:

```csharp
_mpb.SetColor("_EmissionColor", noteColor * 1.8f); // HDR emission for bloom
_mpb.SetColor("_BaseColor",     noteColor);
capsuleMeshRenderer.SetPropertyBlock(_mpb);
```

### 4.3 Object Pool: `NotePool.cs`

```csharp
public class NotePool : MonoBehaviour
{
    [SerializeField] GameObject noteCapsulePrefab;
    [SerializeField] int        initialPoolSize = 64;

    private Queue<NoteView> _available = new Queue<NoteView>();
    private List<NoteView>  _active    = new List<NoteView>();

    void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var go = Instantiate(noteCapsulePrefab, transform);
            go.SetActive(false);
            _available.Enqueue(go.GetComponent<NoteView>());
        }
    }

    public NoteView Rent(NoteData data)
    {
        NoteView view = _available.Count > 0
            ? _available.Dequeue()
            : Instantiate(noteCapsulePrefab, transform).GetComponent<NoteView>();
        view.gameObject.SetActive(true);
        view.Initialize(data);
        _active.Add(view);
        return view;
    }

    public void Return(NoteView view)
    {
        view.gameObject.SetActive(false);
        _active.Remove(view);
        _available.Enqueue(view);
    }
}
```

### 4.4 Scroll Speed Math

Notes travel along the **Z axis** of `FretboardRoot` (world-space X is string lanes, world-space Z is time axis).

```
// Scroll speed derivation:
// HIGHWAY_VISIBLE_SECONDS = 2.5f    (how many seconds of notes are visible at once)
// HIGHWAY_VISIBLE_UNITS   = 10.0f   (Z distance from spawn to hit zone = fretboard visible length)
// SCROLL_SPEED = HIGHWAY_VISIBLE_UNITS / HIGHWAY_VISIBLE_SECONDS = 4.0 units/second

const float HIGHWAY_VISIBLE_SECONDS = 2.5f;
const float HIGHWAY_VISIBLE_UNITS   = 10.0f;
const float SCROLL_SPEED            = HIGHWAY_VISIBLE_UNITS / HIGHWAY_VISIBLE_SECONDS; // = 4.0 u/s

// Spawn position for a note (at time T, current song time = songTime):
float spawnZ = HIT_ZONE_Z + (noteData.songTimeSeconds - songTime) * SCROLL_SPEED;
//   HIT_ZONE_Z = 1.37f (= fretZ[2], see §2.4)
//   Note spawns at high Z and travels toward low Z (camera side)

// Each frame, NoteHighway.Update() moves all active notes:
void Update()
{
    float dt = Time.deltaTime;
    foreach (var note in _pool.ActiveNotes)
        note.transform.localPosition -= new Vector3(0, 0, SCROLL_SPEED * dt);
}
```

**Spawn lookahead:** Notes are spawned when `noteData.songTimeSeconds - songClock.CurrentTime <= LOOKAHEAD_SECONDS` where `LOOKAHEAD_SECONDS = HIGHWAY_VISIBLE_SECONDS + 0.2f = 2.7f`.

**Despawn:** If a note's Z < `HIT_ZONE_Z - 0.5f` and it hasn't been hit, it's returned to pool and scored as a Miss.

### 4.5 String Lane Mapping

Note X position maps directly to `FretboardMeshBuilder.STRING_X[note.stringIndex]`. Y is set to `STRING_Y[note.stringIndex] + 0.02f` (float slightly above string).

---

## 5. Tab/Song Data System

### 5.1 SongData and NoteEvent Structs

```csharp
// Assets/_Project/Scripts/Tabs/SongData.cs
[System.Serializable]
public class SongData
{
    public string   songId;
    public string   title;
    public string   artist;
    public int      bpm;
    public int      beatsPerBar;     // time signature numerator (4)
    public int      beatUnit;        // time signature denominator (4)
    public float    durationSeconds;
    public string   tuning;          // e.g. "EADGBe" or "DADGBe" for drop-D
    public List<NoteEvent> notes;    // sorted ascending by startTimeSec
    public List<BeatMarker> beats;   // bar/beat grid for HUD metronome display
}

[System.Serializable]
public struct NoteEvent
{
    // ── Timing ────────────────────────────────────
    public double  startTimeSec;        // absolute song time, double precision
    public float   durationSec;         // sustain; 0.0625 = 1/16 note at 120bpm

    // ── Pitch / Tab ───────────────────────────────
    public int     stringIndex;         // 0–5
    public int     fretNumber;          // 0–15 (0=open)
    public int     midiNote;            // computed: OPEN_MIDI[string] + fret

    // ── Expression ───────────────────────────────
    public byte    velocity;            // 0–127
    public NoteType noteType;
    public float   bendCents;           // 0=none, 100=full step bend, 200=full tone
    public int     slideTargetFret;     // -1 if not a slide

    // ── Guitar Pro compatibility fields ───────────
    public int     gpxTrackIndex;       // always 0 for single-guitar
    public int     gpxVoiceIndex;       // 0 = main voice
    public int     gpxBeatIndex;        // beat index within bar
    public int     gpxBarIndex;
}

[System.Serializable]
public struct BeatMarker
{
    public int    barIndex;
    public int    beatIndex;         // 0-based within bar
    public double timeSec;
    public bool   isDownbeat;        // beatIndex == 0
}

// Open string MIDI notes (standard tuning, E2=40):
static readonly int[] OPEN_MIDI = { 40, 45, 50, 55, 59, 64 };
// midiNote = OPEN_MIDI[stringIndex] + fretNumber
```

### 5.2 Random Tab Generator

```csharp
// Assets/_Project/Scripts/Tabs/RandomTabGenerator.cs
public static class RandomTabGenerator
{
    // Configuration struct
    public struct GeneratorConfig
    {
        public int   bpm;               // default 90
        public int   barCount;          // default 8
        public int   beatsPerBar;       // default 4
        public int   subdivisions;      // 4=quarter, 8=eighth, 16=sixteenth
        public float noteDensity;       // 0.0–1.0, fraction of subdivisions filled
        public int   maxFret;           // default 7 (first position)
        public int   minFret;           // default 0 (allow open strings)
        public bool  allowOpenStrings;
        public bool  preferPentatonic;  // bias toward minor pentatonic shapes
        public int   rootNote;          // MIDI note of key root (e.g. 40=E)
        public Scale scale;             // enum: MinorPentatonic, Major, NaturalMinor
    }

    // Minor pentatonic intervals (semitones from root): 0,3,5,7,10
    static readonly int[] MINOR_PENTA_INTERVALS = { 0, 3, 5, 7, 10 };
    // Major scale intervals: 0,2,4,5,7,9,11
    static readonly int[] MAJOR_INTERVALS        = { 0, 2, 4, 5, 7, 9, 11 };

    public static SongData Generate(GeneratorConfig cfg, int seed = -1)
    {
        var rng = seed < 0 ? new System.Random() : new System.Random(seed);

        double secondsPerBeat  = 60.0 / cfg.bpm;
        double secondsPerSubdiv = secondsPerBeat / (cfg.subdivisions / cfg.beatsPerBar);
        int    totalSubdivs    = cfg.barCount * cfg.beatsPerBar * (cfg.subdivisions / cfg.beatsPerBar);
        float  totalDuration   = (float)(totalSubdivs * secondsPerSubdiv);

        var song = new SongData
        {
            songId          = $"random_{seed}",
            title           = "Exercise",
            artist          = "Random",
            bpm             = cfg.bpm,
            beatsPerBar     = cfg.beatsPerBar,
            beatUnit        = 4,
            durationSeconds = totalDuration,
            tuning          = "EADGBe",
            notes           = new List<NoteEvent>(),
            beats           = new List<BeatMarker>()
        };

        // Build allowed MIDI note set from scale
        var allowedMidi = BuildAllowedMidiSet(cfg);

        // Generate beat markers
        for (int bar = 0; bar < cfg.barCount; bar++)
        for (int beat = 0; beat < cfg.beatsPerBar; beat++)
        {
            song.beats.Add(new BeatMarker {
                barIndex   = bar,
                beatIndex  = beat,
                timeSec    = (bar * cfg.beatsPerBar + beat) * secondsPerBeat,
                isDownbeat = (beat == 0)
            });
        }

        // Generate notes
        // State machine ensures no two notes share same string within 1 subdivision
        int[] lastFretPerString = new int[6];
        Array.Fill(lastFretPerString, -1);

        for (int subdiv = 0; subdiv < totalSubdivs; subdiv++)
        {
            if (rng.NextDouble() > cfg.noteDensity) continue; // rest

            double startTime = subdiv * secondsPerSubdiv;

            // Pick a string, biasing toward strings not recently used
            int strIdx  = PickString(rng, lastFretPerString);
            int fret    = PickFret(rng, strIdx, allowedMidi, cfg, lastFretPerString[strIdx]);

            if (fret < 0) continue; // no valid fret found

            lastFretPerString[strIdx] = fret;

            // Duration: mostly short (1 subdiv), occasionally held (2–4 subdivs)
            float dur = (float)(secondsPerSubdiv * (rng.NextDouble() < 0.15 ? rng.Next(2, 5) : 1));
            dur = Mathf.Min(dur, totalDuration - (float)startTime - 0.01f);

            song.notes.Add(new NoteEvent {
                startTimeSec  = startTime,
                durationSec   = dur,
                stringIndex   = strIdx,
                fretNumber    = fret,
                midiNote      = FretboardMeshBuilder.OPEN_MIDI[strIdx] + fret,
                velocity      = (byte)rng.Next(80, 115),
                noteType      = NoteType.Normal,
                gpxBarIndex   = subdiv / (cfg.beatsPerBar * (cfg.subdivisions / cfg.beatsPerBar)),
                gpxBeatIndex  = (subdiv / (cfg.subdivisions / cfg.beatsPerBar)) % cfg.beatsPerBar
            });
        }

        song.notes.Sort((a, b) => a.startTimeSec.CompareTo(b.startTimeSec));
        return song;
    }

    static HashSet<int> BuildAllowedMidiSet(GeneratorConfig cfg)
    {
        int[] intervals = cfg.scale == Scale.Major ? MAJOR_INTERVALS : MINOR_PENTA_INTERVALS;
        var set = new HashSet<int>();
        for (int oct = 2; oct <= 6; oct++)
        foreach (int interval in intervals)
            set.Add(cfg.rootNote + interval + oct * 12);
        return set;
    }

    static int PickString(System.Random rng, int[] lastFrets)
    {
        // Weight strings that haven't been played recently higher
        float[] weights = new float[6];
        for (int i = 0; i < 6; i++)
            weights[i] = lastFrets[i] < 0 ? 2.0f : 1.0f;
        return WeightedRandom(rng, weights);
    }

    static int PickFret(System.Random rng, int strIdx, HashSet<int> allowedMidi,
                        GeneratorConfig cfg, int lastFret)
    {
        // Try up to 20 random frets in [cfg.minFret, cfg.maxFret]
        for (int attempt = 0; attempt < 20; attempt++)
        {
            int f = rng.Next(cfg.minFret, cfg.maxFret + 1);
            int midi = FretboardMeshBuilder.OPEN_MIDI[strIdx] + f;
            if (!allowedMidi.Contains(midi)) continue;
            // Prefer frets within ±3 of lastFret (ergonomic constraint)
            if (lastFret >= 0 && Mathf.Abs(f - lastFret) > 5 && rng.NextDouble() < 0.7f) continue;
            return f;
        }
        return -1; // failed
    }

    static int WeightedRandom(System.Random rng, float[] weights)
    {
        float total = 0; foreach (float w in weights) total += w;
        float r = (float)rng.NextDouble() * total;
        for (int i = 0; i < weights.Length; i++) { r -= weights[i]; if (r <= 0) return i; }
        return weights.Length - 1;
    }
}
```

---

## 6. Timing Engine

### 6.1 Architecture

```
SongClock       → authoritative source of truth, backed by DSP time
TimingEngine    → reads SongClock, maintains note queue, issues events
```

### 6.2 SongClock.cs

```csharp
public class SongClock : MonoBehaviour
{
    // DSP-backed clock — avoids Update() jitter (typically ±2–5ms)
    private double _dspStartTime;    // AudioSettings.dspTime at Play()
    private double _songStartOffset; // offset into song if resuming
    private bool   _isRunning;

    public double CurrentTime => _isRunning
        ? (AudioSettings.dspTime - _dspStartTime) + _songStartOffset
        : _songStartOffset;

    public void Play(double offsetSeconds = 0.0)
    {
        _songStartOffset = offsetSeconds;
        _dspStartTime    = AudioSettings.dspTime;
        _isRunning       = true;
    }

    public void Pause() { _songStartOffset = CurrentTime; _isRunning = false; }
    public void Stop()  { _songStartOffset = 0.0; _isRunning = false; }
}
```

**Why DSP time?** `AudioSettings.dspTime` increments at audio sample granularity (~0.37ms at 44100Hz / 16-sample buffer). `Time.time` has Update-loop jitter of 5–16ms. For a 60fps game, DSP time gives ~40× better resolution.

### 6.3 Timing Windows

```csharp
// Assets/_Project/Scripts/Core/TimingEngine.cs
public static class TimingWindows
{
    // All values in seconds (not ms, to match SongClock type)
    public const double PERFECT_WINDOW_SEC = 0.045;  // ±45ms  → "Perfect"
    public const double GOOD_WINDOW_SEC    = 0.090;  // ±90ms  → "Good"
    public const double LATE_WINDOW_SEC    = 0.135;  // ±135ms → "Late" (still counts)
    // Outside LATE_WINDOW_SEC = Miss

    public static HitQuality EvaluateHit(double noteTimeSec, double hitTimeSec)
    {
        double delta = Math.Abs(hitTimeSec - noteTimeSec);
        if (delta <= PERFECT_WINDOW_SEC) return HitQuality.Perfect;
        if (delta <= GOOD_WINDOW_SEC)    return HitQuality.Good;
        if (delta <= LATE_WINDOW_SEC)    return HitQuality.Late;
        return HitQuality.Miss;
    }
}

public enum HitQuality { Perfect, Good, Late, Miss }
```

**Reference:**
- Yousician uses approximately ±50ms for "Perfect" on guitar.
- Rock Band 3 Pro Guitar uses ±43ms Perfect, ±87ms Good.
- Our ±45ms / ±90ms splits the difference and matches iOS AudioUnit latency budget.

### 6.4 Note Queue Management

```csharp
// TimingEngine maintains a sorted queue of upcoming notes
// Each frame:
public void Update()
{
    double now = _songClock.CurrentTime;

    // 1. Advance note-spawn pointer (hand to NoteHighway)
    while (_spawnPointer < _song.notes.Count &&
           _song.notes[_spawnPointer].startTimeSec - now <= SPAWN_LOOKAHEAD_SEC)
    {
        _noteHighway.SpawnNote(_song.notes[_spawnPointer]);
        _spawnPointer++;
    }

    // 2. Auto-miss notes past the late window
    foreach (var active in _noteHighway.ActiveNotes.ToList())
    {
        if (now - active.Data.startTimeSec > TimingWindows.LATE_WINDOW_SEC + 0.016)
        {
            _scoreManager.RegisterMiss(active.Data);
            _noteHighway.Pool.Return(active);
        }
    }
}
```

---

## 7. Input Pipeline

### 7.1 IInputProvider Interface

```csharp
// Assets/_Project/Scripts/Input/IInputProvider.cs
public interface IInputProvider
{
    string ProviderId { get; }
    bool   IsAvailable { get; }

    // Called once per frame; implementations post events to InputRouter
    void PollFrame(double currentSongTime);

    // Lifecycle
    void Initialize();
    void Shutdown();
}

// Event posted to InputRouter
public struct InputNoteEvent
{
    public int    stringIndex;   // -1 if not determinable (mic pitch detection)
    public int    fretNumber;    // -1 if not determinable
    public int    midiNote;      // always set
    public double timestamp;     // AudioSettings.dspTime at detection moment
    public float  velocity;      // 0.0–1.0
    public InputSourceType source;
}

public enum InputSourceType { Keyboard, Microphone, MIDI, CoreML }
```

### 7.2 KeyboardInputProvider (Prototype)

```csharp
// Keyboard layout simulates frets 0–7 on strings 0–5:
// String select:  keys Q/W/E/R/T/Y  → strings 5,4,3,2,1,0 (high e → low E)
// Fret select:    keys 1–8          → frets 0–7
// Strum/confirm:  SPACE or RETURN

public class KeyboardInputProvider : MonoBehaviour, IInputProvider
{
    public string ProviderId => "keyboard";
    public bool   IsAvailable => Application.isEditor || Debug.isDebugBuild;

    private static readonly KeyCode[] STRING_KEYS = {
        KeyCode.Y, KeyCode.T, KeyCode.R, KeyCode.E, KeyCode.W, KeyCode.Q
    };
    private static readonly KeyCode[] FRET_KEYS = {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8
    };

    private int _selectedString = 0;
    private int _selectedFret   = 0;

    public void PollFrame(double songTime)
    {
        for (int s = 0; s < 6; s++)
            if (Input.GetKeyDown(STRING_KEYS[s])) _selectedString = s;
        for (int f = 0; f < 8; f++)
            if (Input.GetKeyDown(FRET_KEYS[f]))   _selectedFret   = f;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            InputRouter.Instance.PostEvent(new InputNoteEvent {
                stringIndex = _selectedString,
                fretNumber  = _selectedFret,
                midiNote    = FretboardMeshBuilder.OPEN_MIDI[_selectedString] + _selectedFret,
                timestamp   = AudioSettings.dspTime,
                velocity    = 0.85f,
                source      = InputSourceType.Keyboard
            });
        }
    }

    public void Initialize() { }
    public void Shutdown()   { }
}
```

### 7.3 MicrophoneInputProvider + CoreML Stub

```csharp
public class MicrophoneInputProvider : MonoBehaviour, IInputProvider
{
    public string ProviderId => "microphone";
    public bool   IsAvailable => Microphone.devices.Length > 0;

    // Audio ring buffer: 4096 samples @ 44100Hz = 92.9ms lookahead
    private const int   SAMPLE_RATE    = 44100;
    private const int   BUFFER_SAMPLES = 4096;
    private const int   HOP_SAMPLES    = 512;  // analysis hop = 11.6ms

    private AudioClip   _micClip;
    private int         _lastReadPos;
    private float[]     _analysisBuffer = new float[BUFFER_SAMPLES];
    private CoreMLPitchDetector _cnnDetector;

    public void Initialize()
    {
        _micClip    = Microphone.Start(null, true, 10, SAMPLE_RATE);
        _cnnDetector = GetComponent<CoreMLPitchDetector>();
        _cnnDetector.LoadModel("PitchDetector"); // loads StreamingAssets/CoreML/PitchDetector.mlpackage
    }

    public void PollFrame(double songTime)
    {
        int micPos = Microphone.GetPosition(null);
        if (micPos < _lastReadPos) _lastReadPos = 0; // wrap

        while (micPos - _lastReadPos >= HOP_SAMPLES)
        {
            _micClip.GetData(_analysisBuffer, _lastReadPos);
            float rms = ComputeRMS(_analysisBuffer, HOP_SAMPLES);
            if (rms > 0.01f) // gate: ignore silence
            {
                // CoreML inference: returns (midiNote:int, confidence:float)
                var (midi, confidence) = _cnnDetector.Infer(_analysisBuffer, HOP_SAMPLES, SAMPLE_RATE);
                if (confidence > 0.72f)
                {
                    InputRouter.Instance.PostEvent(new InputNoteEvent {
                        stringIndex = -1,               // mic can't determine string
                        fretNumber  = -1,
                        midiNote    = midi,
                        timestamp   = AudioSettings.dspTime,
                        velocity    = Mathf.Clamp01(rms * 8f),
                        source      = InputSourceType.CoreML
                    });
                }
            }
            _lastReadPos += HOP_SAMPLES;
        }
    }

    private float ComputeRMS(float[] buf, int len)
    {
        float sum = 0; for (int i = 0; i < len; i++) sum += buf[i] * buf[i];
        return Mathf.Sqrt(sum / len);
    }

    public void Shutdown() => Microphone.End(null);
}

// CoreMLPitchDetector.cs — platform-specific bridge stub
public class CoreMLPitchDetector : MonoBehaviour
{
    // Marshals float[] to native CoreML model via P/Invoke on iOS
    // On Android / Editor: falls back to lightweight YIN pitch detection

    [DllImport("__Internal")]
    private static extern int CoreML_InferPitch(float[] pcm, int len, int sampleRate, out float confidence);

    public (int midiNote, float confidence) Infer(float[] pcm, int len, int sampleRate)
    {
#if UNITY_IOS && !UNITY_EDITOR
        float conf;
        int midi = CoreML_InferPitch(pcm, len, sampleRate, out conf);
        return (midi, conf);
#else
        return YINFallback.Detect(pcm, len, sampleRate); // software fallback
#endif
    }

    public void LoadModel(string modelName)
    {
        // iOS: model loaded by Objective-C layer at app launch
        // Android/Editor: no-op, YIN fallback used
    }
}
```

### 7.4 MIDI Input Stub

```csharp
public class MIDIInputProvider : MonoBehaviour, IInputProvider
{
    public string ProviderId => "midi";
    // iOS: CoreMIDI via ObjC plugin; Android: USB MIDI via Java plugin
    public bool IsAvailable => false; // set to true when plugin is integrated

    public void Initialize()
    {
        // Placeholder: register CoreMIDI callback via NativeBridge
        // NativeBridge.SendMessage("RegisterMIDI", "");
    }

    // Called from native layer via UnitySendMessage:
    // NativeBridge receives "OnMIDINote", data = "{note:64,velocity:100,channel:1}"
    public void OnMIDINoteReceived(string jsonData)
    {
        var msg = JsonUtility.FromJson<MIDINoteMessage>(jsonData);
        InputRouter.Instance.PostEvent(new InputNoteEvent {
            stringIndex = -1,
            fretNumber  = -1,
            midiNote    = msg.note,
            timestamp   = AudioSettings.dspTime,
            velocity    = msg.velocity / 127f,
            source      = InputSourceType.MIDI
        });
    }

    public void PollFrame(double songTime) { /* events pushed by callback */ }
    public void Shutdown() { }
}
```

### 7.5 InputRouter

```csharp
public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance { get; private set; }

    [SerializeField] KeyboardInputProvider  keyboardProvider;
    [SerializeField] MicrophoneInputProvider micProvider;
    [SerializeField] MIDIInputProvider       midiProvider;

    // Consumers (TimingEngine registers here)
    public event System.Action<InputNoteEvent> OnNoteEvent;

    void Awake()  { Instance = this; }
    void Start()  { keyboardProvider.Initialize(); micProvider.Initialize(); midiProvider.Initialize(); }
    void Update() {
        double t = FindObjectOfType<SongClock>().CurrentTime;
        keyboardProvider.PollFrame(t);
        micProvider.PollFrame(t);
        // midiProvider is callback-driven
    }

    public void PostEvent(InputNoteEvent evt) => OnNoteEvent?.Invoke(evt);
}
```

---

## 8. Hit Detection & Scoring

### 8.1 Hit Matching

```csharp
// TimingEngine.OnNoteInput() — called by InputRouter.OnNoteEvent
private void OnNoteInput(InputNoteEvent input)
{
    double now = _songClock.CurrentTime;

    // Find closest active note matching MIDI pitch
    NoteView best      = null;
    double   bestDelta = double.MaxValue;

    foreach (var noteView in _noteHighway.ActiveNotes)
    {
        int expectedMidi = FretboardMeshBuilder.OPEN_MIDI[noteView.Data.stringIndex]
                           + noteView.Data.fretNumber;
        if (expectedMidi != input.midiNote) continue;

        double delta = Math.Abs(noteView.Data.startTimeSec - now);
        if (delta < bestDelta && delta <= TimingWindows.LATE_WINDOW_SEC)
        {
            bestDelta = delta;
            best      = noteView;
        }
    }

    if (best != null)
        _scoreManager.RegisterHit(best.Data, input, now);
    else
        _scoreManager.RegisterGhost(input); // played a note with nothing to hit → ghost penalty
}
```

### 8.2 Score Formula

```csharp
public class ScoreManager : MonoBehaviour
{
    // Base points per hit quality
    public const int BASE_PERFECT = 100;
    public const int BASE_GOOD    = 60;
    public const int BASE_LATE    = 30;
    public const int MISS_PENALTY = 0;   // no negative, just no points

    // Multiplier thresholds (consecutive non-miss hits)
    // Streak 0–9   → ×1
    // Streak 10–19 → ×2
    // Streak 20–29 → ×3
    // Streak 30+   → ×4
    private static int[] STREAK_THRESHOLDS = { 0, 10, 20, 30 };

    private long  _score;
    private int   _streak;         // consecutive non-miss hits
    private int   _multiplier = 1;
    private int   _totalNotes;
    private int   _hitCount;
    private int   _perfectCount;

    public long  Score       => _score;
    public int   Multiplier  => _multiplier;
    public float Accuracy    => _totalNotes > 0 ? (float)_hitCount / _totalNotes : 1f;

    public void RegisterHit(NoteData note, InputNoteEvent input, double hitTime)
    {
        var quality = TimingWindows.EvaluateHit(note.songTimeSeconds, hitTime);
        int basePoints = quality switch {
            HitQuality.Perfect => BASE_PERFECT,
            HitQuality.Good    => BASE_GOOD,
            HitQuality.Late    => BASE_LATE,
            _                  => 0
        };

        // Velocity bonus: up to +20% for velocity near 100
        float velBonus = 1f + (input.velocity - 0.5f) * 0.2f;
        int   points   = (int)(basePoints * _multiplier * velBonus);

        _score   += points;
        _streak++;
        _hitCount++;
        _totalNotes++;
        _multiplier = ComputeMultiplier(_streak);

        OnHitRegistered?.Invoke(note, quality, points, _multiplier);
    }

    public void RegisterMiss(NoteData note)
    {
        _streak      = 0;
        _multiplier  = 1;
        _totalNotes++;
        OnMissRegistered?.Invoke(note);
    }

    public void RegisterGhost(InputNoteEvent input)
    {
        // Ghost notes do not break streak but are tracked for accuracy display
    }

    private int ComputeMultiplier(int streak)
    {
        for (int i = STREAK_THRESHOLDS.Length - 1; i >= 0; i--)
            if (streak >= STREAK_THRESHOLDS[i]) return i + 1;
        return 1;
    }

    public event System.Action<NoteData, HitQuality, int, int> OnHitRegistered;
    public event System.Action<NoteData>                        OnMissRegistered;
}
```

---

## 9. Feedback System

### 9.1 FloatingTextSpawner

```csharp
// Spawns "PERFECT", "GOOD", "LATE", "MISS" text at hit zone screen position
// FloatingText prefab: TextMeshProUGUI in world space, font=72pt Impact-style
public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] GameObject floatingTextPrefab;
    [SerializeField] int        poolSize = 12;
    [SerializeField] Transform  hitZoneWorldPos;

    private Queue<FloatingTextView> _pool = new Queue<FloatingTextView>();

    void Start() { /* pre-warm pool */ }

    public void Spawn(HitQuality quality, Vector3 worldPos)
    {
        string  text  = quality.ToString().ToUpper();
        Color   color = quality switch {
            HitQuality.Perfect => new Color(1f, 0.9f, 0.1f),   // gold
            HitQuality.Good    => new Color(0.3f, 1f, 0.4f),   // green
            HitQuality.Late    => new Color(1f, 0.5f, 0.1f),   // orange
            HitQuality.Miss    => new Color(1f, 0.2f, 0.2f),   // red
            _                  => Color.white
        };

        var view = RentFromPool();
        view.Play(text, color, worldPos);
    }

    // FloatingTextView animation: DOTween sequence
    // 0.0s: scale 0 → 1.2 (0.08s, Ease.OutBack)
    // 0.08s: scale 1.2 → 1.0 (0.04s)
    // 0.12s: hold + drift upward 0.3u over 0.4s
    // 0.52s: fade alpha 1→0 over 0.15s
    // Total lifetime: 0.67s
}
```

### 9.2 String Vibration Shader

The custom `S_StringVibration.shader` animates string vertices:

```hlsl
// S_StringVibration.shader (URP Unlit, custom vertex stage)
// Properties:
//   _VibrationAmplitude ("Amplitude", Range(0,0.015)) = 0.0
//   _VibrationFreq      ("Frequency Hz", Range(80, 1200)) = 440.0
//   _VibrationDecay     ("Decay Rate", Range(1, 30)) = 12.0
//   _VibrationStartTime ("Start Time", Float) = -999.0

Varyings vert(Attributes input)
{
    float elapsed   = _Time.y - _VibrationStartTime;
    float envelope  = exp(-_VibrationDecay * max(elapsed, 0.0));
    float sine      = sin(2.0 * PI * _VibrationFreq * elapsed);
    float offset    = _VibrationAmplitude * envelope * sine;

    // Only displace Y (up-down); use UV.x as phase offset along string length
    float phase     = sin(input.texcoord.x * PI); // standing wave shape
    input.positionOS.y += offset * phase;

    // ... standard URP transform
}
```

`StringVibrationAnimator.cs` sets `_VibrationStartTime = Time.timeSinceLevelLoad` and `_VibrationFreq` = MIDI note frequency via `MaterialPropertyBlock` when a string is hit.

### 9.3 Camera Shake

```csharp
public class CameraShakeController : MonoBehaviour
{
    // Perlin noise shake parameters per quality tier
    public struct ShakeProfile
    {
        public float duration;    // seconds
        public float magnitude;   // world units
        public float frequency;   // noise frequency
    }

    static readonly ShakeProfile[] SHAKE_PROFILES = {
        new ShakeProfile { duration=0.12f, magnitude=0.005f, frequency=25f }, // Perfect
        new ShakeProfile { duration=0.08f, magnitude=0.003f, frequency=20f }, // Good
        new ShakeProfile { duration=0.05f, magnitude=0.002f, frequency=15f }, // Late
        new ShakeProfile { duration=0.20f, magnitude=0.010f, frequency=10f }, // Miss
    };

    private float _shakeTimer;
    private float _shakeMag;
    private float _shakeFreq;

    public void TriggerShake(HitQuality quality)
    {
        var p   = SHAKE_PROFILES[(int)quality];
        _shakeTimer = p.duration;
        _shakeMag   = p.magnitude;
        _shakeFreq  = p.frequency;
    }

    void Update()
    {
        if (_shakeTimer <= 0f) { transform.localPosition = Vector3.zero; return; }
        _shakeTimer -= Time.deltaTime;
        float t     = _shakeTimer;
        float x     = (Mathf.PerlinNoise(t * _shakeFreq,       0.5f) - 0.5f) * 2f * _shakeMag;
        float y     = (Mathf.PerlinNoise(0.5f, t * _shakeFreq + 1f) - 0.5f) * 2f * _shakeMag;
        transform.localPosition = new Vector3(x, y, 0f);
    }
}
```

### 9.4 Hit Particle Effects

**`HitParticleSystem.prefab`** — parented to `HitParticleRoot`, plays at hit zone world position, string Y:

| Parameter | Perfect | Good | Miss |
|---|---|---|---|
| Burst count | 22 | 12 | 6 |
| Start speed | 2.5–5.0 | 1.5–3.5 | 0.8–2.0 |
| Start lifetime | 0.4–0.7s | 0.3–0.5s | 0.2–0.4s |
| Start size | 0.02–0.05 | 0.015–0.04 | 0.01–0.02 |
| Color | Gold HDR (3× intensity) | Green HDR (2×) | Red (1×) |
| Shape | Sphere radius 0.05 | Sphere 0.04 | Sphere 0.03 |
| Renderer | Billboard, Additive blend | Additive | Additive |

`ParticleHitEffect.cs` maintains one `ParticleSystem` per string (6 total, pre-positioned at each string Y).

---

## 10. HUD

### 10.1 Unity-Rendered HUD Elements

All HUD lives on `HUDCanvas` (Screen Space – Overlay, render order 100) driven by `HUDController.cs`. Unity renders:

- **Score:** `TextMeshProUGUI`, top-left, Roboto-Mono Bold 36pt, anchored (0, 1)
  - Format: `"SCORE\n{score:N0}"` — updates every frame via `ScoreManager.Score`
- **Multiplier badge:** `Image` (circular badge) + centered `TextMeshProUGUI` "×{mult}"
  - Animates scale: 1.0 → 1.35 → 1.0 over 0.15s on multiplier change (DOTween)
  - Color ramp: ×1=white, ×2=yellow, ×3=orange, ×4=red-gold
- **Accuracy bar:** `Slider` component, min=0, max=1, value=`ScoreManager.Accuracy`
  - Fill color gradient: green (1.0) → yellow (0.7) → orange (0.5) → red (0.3)
  - Updates every 0.25s (not every frame) to avoid layout rebuild cost
- **Streak counter:** Small `TextMeshProUGUI` below multiplier, shows current streak count

### 10.2 Swift-Rendered Elements (Overlay Native Views)

The following are **native UIKit views** drawn above the Unity view:

- **Settings button** (top-right corner): `UIButton` with gear icon — taps send `settings_open` to Swift via NativeBridge
- **Back/Exit button** (top-left): `UIButton` — sends `exercise_exit` via NativeBridge
- **Song title label** (top-center, first 3s only): shown on exercise start, fades out

These are transparent overlays positioned via `UIView.frame` to match the top 20% HUD strip.

### 10.3 HUD ↔ Score Messaging (Unity → Swift)

Every 1.0 second, `HUDController` serializes a `HUDStateSnapshot` and sends it to Swift:

```csharp
[System.Serializable]
public struct HUDStateSnapshot
{
    public long  score;
    public int   multiplier;
    public float accuracy;      // 0.0–1.0
    public int   streak;
    public float songProgress;  // 0.0–1.0
}

// In HUDController:
private float _lastSnapshotTime;
void Update()
{
    if (Time.time - _lastSnapshotTime >= 1.0f)
    {
        _lastSnapshotTime = Time.time;
        var snap = new HUDStateSnapshot { ... };
        NativeBridge.SendToNative("hud_state_update", JsonUtility.ToJson(snap));
    }
}
```

---

## 11. Red Rocks Background

### 11.1 Layer Breakdown

Three pre-rendered PNG layers (created in Photoshop / Procreate):

| Layer | File | Dimensions | Content | Depth Factor |
|---|---|---|---|---|
| 0 (back) | `RedRocks_Layer0_Sky.png` | 2560×720 | Gradient sky (deep purple→orange horizon), stars, moon | 0.0 (fixed) |
| 1 (mid) | `RedRocks_Layer1_Rocks.png` | 2560×720 | Iconic Red Rocks sandstone formations silhouette | 0.3 |
| 2 (front) | `RedRocks_Layer2_Crowd.png` | 2560×720 | Crowd silhouette with colored wristbands | 0.6 |

Layers are rendered on a `Canvas` set to **World Space**, positioned at Z=6.0 (behind fretboard at Z≈0–12). Layer quads are scaled to fill the middle strip of the viewport.

### 11.2 Parallax Math

```csharp
// ParallaxBackground.cs
// The parallax reacts to:
// (a) Gyroscope tilt (±5° range → ±0.12 units horizontal shift)
// (b) Camera shake offset (shared from CameraShakeController)
// (c) Subtle auto-sway: sin wave, amplitude=0.02u, period=8s

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] RectTransform[] layers;       // [3], index 0=sky, 2=crowd
    [SerializeField] float[]         depthFactors = { 0.0f, 0.3f, 0.6f };

    // Gyroscope baseline (set at scene load, re-zeroed on resume)
    private Vector3 _gyroBaseline;

    // Base positions (set at Awake)
    private Vector2[] _basePositions;

    const float GYRO_SENSITIVITY    = 0.025f;  // units per degree of tilt
    const float GYRO_MAX_OFFSET_DEG = 5.0f;
    const float AUTO_SWAY_AMP       = 0.02f;
    const float AUTO_SWAY_PERIOD    = 8.0f;

    void LateUpdate()
    {
        float gyroX = Input.gyro.enabled
            ? Mathf.Clamp((Input.gyro.attitude.eulerAngles.y - _gyroBaseline.y) * GYRO_SENSITIVITY,
                          -GYRO_MAX_OFFSET_DEG * GYRO_SENSITIVITY,
                           GYRO_MAX_OFFSET_DEG * GYRO_SENSITIVITY)
            : 0f;

        float sway = Mathf.Sin(Time.time * (2f * Mathf.PI / AUTO_SWAY_PERIOD)) * AUTO_SWAY_AMP;
        float totalX = gyroX + sway;

        for (int i = 0; i < layers.Length; i++)
        {
            float parallaxShift = totalX * depthFactors[i];
            layers[i].anchoredPosition = _basePositions[i] + new Vector2(parallaxShift, 0f);
        }
    }
}
```

**Texture bleeding prevention:** Each layer PNG has 40px of extra content on left/right edges (beyond visible area) so parallax shift of up to ±0.06 unit never shows empty edges.

---

## 12. Unity ↔ Swift Bridge

### 12.1 Architecture

```
[Unity C#]                          [iOS Swift / Android Java]
NativeBridge.cs
  ├── SendToNative(key, payload)  →  Plugin .mm / .java
  │                                   └── Posts to native message handler
  └── ReceiveFromNative(key, payload) ← UnitySendMessage(gameObjectName, methodName, payload)
```

`NativeBridge` is a singleton `MonoBehaviour` (DontDestroyOnLoad) on a GameObject named **exactly** `"NativeBridge"` — this string is used by `UnitySendMessage`.

### 12.2 Complete Message Catalog

```
Direction  Key                     Payload (JSON)                      Description
─────────────────────────────────────────────────────────────────────────────────────────
U → N      exercise_ready          {}                                  Unity scene fully loaded
U → N      exercise_complete       {score, accuracy, stars, duration}  Song finished
U → N      exercise_exit_ack       {}                                  Confirmed exit, safe to pop VC
U → N      hud_state_update        {score, multiplier, accuracy,       Periodic HUD sync (1Hz)
                                    streak, songProgress}
U → N      error                   {code, message}                     Runtime error report
U → N      skin_change_request     {skinId}                            Player opened skin picker
U → N      settings_open           {}                                  Settings button tapped

N → U      start_exercise          {songId, skinId, bpm, difficulty}   Begin exercise
N → U      pause_exercise          {}                                  App backgrounded / pause btn
N → U      resume_exercise         {}                                  Resume from pause
N → U      stop_exercise           {}                                  Hard stop (navigate away)
N → U      apply_skin              {skinId}                            Skin selected in native UI
N → U      set_input_mode          {mode: "keyboard"|"mic"|"midi"}     Switch input provider
N → U      set_scroll_speed        {speedMultiplier: float 0.5–2.0}    Highway speed override
N → U      set_volume              {sfxVol: 0–1, musicVol: 0–1}        Audio mix
```

### 12.3 NativeBridge.cs

```csharp
// Assets/_Project/Scripts/Bridge/NativeBridge.cs
public class NativeBridge : MonoBehaviour
{
    public static NativeBridge Instance { get; private set; }

    // Events consumed by other Unity systems
    public event System.Action<string, string> OnMessageFromNative; // (key, payload)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.name = "NativeBridge"; // MUST match UnitySendMessage target
    }

    // ── Unity → Native ────────────────────────────────────────────────────────
    public static void SendToNative(string key, string payloadJson = "{}")
    {
#if UNITY_IOS && !UNITY_EDITOR
        _iOS_PostMessage(key, payloadJson);
#elif UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass("com.example.guitarapp.NativeBridgePlugin");
        jc.CallStatic("receiveFromUnity", key, payloadJson);
#else
        Debug.Log($"[NativeBridge → Native] {key}: {payloadJson}");
#endif
    }

    // ── Native → Unity (called via UnitySendMessage) ──────────────────────────
    // Method name is the exact string passed as third arg to UnitySendMessage
    public void OnNativeMessage(string rawJson)
    {
        var msg = JsonUtility.FromJson<NativeMessage>(rawJson);
        OnMessageFromNative?.Invoke(msg.key, msg.payload);
        DispatchMessage(msg.key, msg.payload);
    }

    private void DispatchMessage(string key, string payload)
    {
        switch (key)
        {
            case "start_exercise":
                var startMsg = JsonUtility.FromJson<StartExercisePayload>(payload);
                GameStateMachine.Instance.StartExercise(startMsg);
                break;
            case "pause_exercise":
                GameStateMachine.Instance.Pause();
                break;
            case "resume_exercise":
                GameStateMachine.Instance.Resume();
                break;
            case "stop_exercise":
                GameStateMachine.Instance.Stop();
                break;
            case "apply_skin":
                var skinMsg = JsonUtility.FromJson<ApplySkinPayload>(payload);
                var skin = Resources.Load<GuitarSkin>($"Skins/{skinMsg.skinId}");
                FindObjectOfType<GuitarSkinApplicator>()?.ApplySkin(skin);
                break;
            case "set_input_mode":
                var inputMsg = JsonUtility.FromJson<SetInputModePayload>(payload);
                InputRouter.Instance.SetActiveProvider(inputMsg.mode);
                break;
            case "set_scroll_speed":
                var speedMsg = JsonUtility.FromJson<SetScrollSpeedPayload>(payload);
                NoteHighway.Instance.ScrollSpeedMultiplier = speedMsg.speedMultiplier;
                break;
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _iOS_PostMessage(string key, string payload);
#endif

    // Payload structs
    [System.Serializable] struct NativeMessage { public string key; public string payload; }
    [System.Serializable] struct StartExercisePayload { public string songId; public string skinId; public int bpm; public int difficulty; }
    [System.Serializable] struct ApplySkinPayload { public string skinId; }
    [System.Serializable] struct SetInputModePayload { public string mode; }
    [System.Serializable] struct SetScrollSpeedPayload { public float speedMultiplier; }
}
```

### 12.4 iOS Native Plugin — `NativeBridgePlugin.mm`

```objc
// Plugins/iOS/NativeBridgePlugin.mm
#import <Foundation/Foundation.h>
#import "UnityFramework/UnityFramework.h"

// Called from Unity via P/Invoke: _iOS_PostMessage(key, payload)
extern "C" void _iOS_PostMessage(const char* key, const char* payload)
{
    NSString* keyStr     = [NSString stringWithUTF8String:key];
    NSString* payloadStr = [NSString stringWithUTF8String:payload];

    // Post to main thread via NotificationCenter → Swift picks this up
    dispatch_async(dispatch_get_main_queue(), ^{
        [[NSNotificationCenter defaultCenter]
            postNotificationName:@"UnityToNative"
            object:nil
            userInfo:@{@"key": keyStr, @"payload": payloadStr}];
    });
}

// Called from Swift to send message to Unity
extern "C" void Native_SendToUnity(const char* key, const char* payload)
{
    NSString* keyStr     = [NSString stringWithUTF8String:key];
    NSString* payloadStr = [NSString stringWithUTF8String:payload];

    NSDictionary* msg = @{@"key": keyStr, @"payload": payloadStr};
    NSData* data      = [NSJSONSerialization dataWithJSONObject:msg options:0 error:nil];
    NSString* json    = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];

    // UnitySendMessage(gameObject, method, message)
    UnityFramework.getInstance().sendMessageToGOWithName(
        "NativeBridge", "OnNativeMessage", [json UTF8String]);
}
```

### 12.5 Swift Side — `UnityBridge.swift`

```swift
// Swift — UnityBridge.swift
import Foundation

final class UnityBridge {
    static let shared = UnityBridge()

    // Outbound: Swift → Unity
    func send(key: String, payload: [String: Any] = [:]) {
        guard let data    = try? JSONSerialization.data(withJSONObject: payload),
              let payStr  = String(data: data, encoding: .utf8) else { return }
        Native_SendToUnity(key, payStr)
    }

    // Inbound: Unity → Swift (registered in AppDelegate / SceneDelegate)
    func startListening() {
        NotificationCenter.default.addObserver(
            self,
            selector: #selector(onUnityMessage(_:)),
            name: NSNotification.Name("UnityToNative"),
            object: nil
        )
    }

    @objc private func onUnityMessage(_ notification: Notification) {
        guard let key     = notification.userInfo?["key"]     as? String,
              let payload = notification.userInfo?["payload"] as? String else { return }
        handleMessage(key: key, payload: payload)
    }

    private func handleMessage(key: String, payload: String) {
        switch key {
        case "exercise_ready":
            NotificationCenter.default.post(name: .exerciseReady, object: nil)

        case "exercise_complete":
            if let data = payload.data(using: .utf8),
               let json = try? JSONDecoder().decode(ExerciseResult.self, from: data) {
                NotificationCenter.default.post(name: .exerciseComplete, object: json)
            }

        case "settings_open":
            DispatchQueue.main.async {
                // Push settings VC from wherever we are
                AppCoordinator.shared.presentSettings()
            }

        case "hud_state_update":
            // Update any native overlay if needed
            break

        default:
            print("[UnityBridge] Unknown key: \(key)")
        }
    }
}

// Strongly-typed result
struct ExerciseResult: Codable {
    let score:    Int
    let accuracy: Float
    let stars:    Int        // 1–3
    let duration: Float
}

extension Notification.Name {
    static let exerciseReady    = Notification.Name("exerciseReady")
    static let exerciseComplete = Notification.Name("exerciseComplete")
}
```

### 12.6 Launching Unity from Swift

```swift
// GuitarExerciseViewController.swift
import UIKit

class GuitarExerciseViewController: UIViewController {

    private var unityView: UIView?

    override func viewDidLoad() {
        super.viewDidLoad()
        view.backgroundColor = .black
        embedUnityView()
        UnityBridge.shared.startListening()

        // Wait for Unity "exercise_ready" then start
        NotificationCenter.default.addObserver(
            self,
            selector: #selector(onExerciseReady),
            name: .exerciseReady,
            object: nil
        )
    }

    private func embedUnityView() {
        let ufw = UnityFramework.getInstance()!
        ufw.setDataBundleId("com.unity3d.framework")
        ufw.runEmbedded(withArgc: CommandLine.argc,
                        argv:     CommandLine.unsafeArgv,
                        appLaunchOpts: nil)

        guard let uv = ufw.appController()?.rootView else { return }
        uv.frame         = view.bounds
        uv.autoresizingMask = [.flexibleWidth, .flexibleHeight]
        view.addSubview(uv)
        unityView = uv
    }

    @objc private func onExerciseReady() {
        // Send exercise config to Unity
        UnityBridge.shared.send(key: "start_exercise", payload: [
            "songId":     "random_42",
            "skinId":     "Skin_Default",
            "bpm":        90,
            "difficulty": 1
        ])
    }

    override var supportedInterfaceOrientations: UIInterfaceOrientationMask {
        return .landscape   // force landscape for Unity scene
    }
}
```

---

## 13. Prototype Milestones

### Milestone 1 — Static Fretboard in Unity (2 days)
**Goal:** Procedural fretboard mesh rendering in URP with correct camera angle.

**Deliverables:**
- `FretboardMeshBuilder.BuildFretboardMesh()` generating correct geometry with taper
- `FretboardMeshBuilder.BuildStringMesh()` for all 6 strings
- Camera rig at specified position/angles (see §2.4)
- `GuitarSkin` ScriptableObject with rosewood, maple, ebony assets wired up
- `GuitarSkinApplicator.ApplySkin()` working with `MaterialPropertyBlock`
- Red Rocks parallax background composited behind fretboard (3 layers)

**Exit Criteria:**
- Screenshot shows fretboard occupying bottom ~40% of a 1920×1080 landscape render
- Pressing keys 1/2/3 in editor switches between wood types with zero GC allocation (verified via Unity Profiler Memory module)
- All 6 strings visible, correctly spaced, with correct gauge widths
- Parallax layers shift correctly when `Input.gyro` is mocked with a Slider in editor UI

---

### Milestone 2 — Note Highway + Timing Engine (3 days)
**Goal:** Notes scroll in sync with DSP clock; pool operates correctly.

**Deliverables:**
- `SongClock` backed by `AudioSettings.dspTime`
- `RandomTabGenerator.Generate()` producing valid `SongData` at 90 BPM, 8 bars, E minor pentatonic
- `NotePool` with 64 pre-warmed `NoteCapsule` instances
- `NoteHighway` spawning, scrolling, and despawning notes correctly
- `HitZonePlane` visible at correct screen position (22% from left)
- Fret numbers readable on capsules at all string positions

**Exit Criteria:**
- `NoteHighway.ActiveNotes.Count` never exceeds 32 during an 8-bar exercise
- Zero `Instantiate` calls after first bar (verified via Profiler Allocation Tracker)
- DSP clock drift vs. `Time.time` demonstrated via on-screen debug overlay: DSP jitter < 1ms/frame, Time.time jitter 3–8ms/frame
- 8-bar run completes with all notes auto-missed and `ScoreManager.Score == 0` (base case)

---

### Milestone 3 — Keyboard Input + Hit Detection + Scoring (2 days)
**Goal:** Playable exercise with keyboard input, score accumulates correctly.

**Deliverables:**
- `KeyboardInputProvider` fully wired: Q/W/E/R/T/Y for strings, 1–8 for frets, Space to strum
- `InputRouter.OnNoteEvent` connected to `TimingEngine.OnNoteInput`
- `TimingWindows.EvaluateHit()` returning correct quality tiers
- `ScoreManager` accumulating score with multiplier
- `FloatingTextSpawner` showing PERFECT/GOOD/LATE/MISS with correct animation
- `StringVibrationAnimator` triggering on hit (correct frequency per MIDI note)
- Camera shake triggering with correct profiles per quality tier
- Hit particle bursts at correct string Y positions

**Exit Criteria:**
- On a generated 8-bar exercise: hitting all notes "on time" (keyboard, simulated) produces score ≥ 800 × note_count (×4 multiplier for 30+ streak, mostly Perfect)
- ±45ms window verified: automated test logs 1000 synthetic hits at exact `startTimeSec` → 100% rated Perfect
- Multiplier badge animates on multiplier change (verified visually in editor play mode)
- `ScoreManager.Accuracy` = 1.0 after perfect run, 0.0 after all misses

---

### Milestone 4 — Unity as Library + Swift Shell Integration (3 days)
**Goal:** Unity embedded in a native iOS app; bridge messaging fully operational.

**Deliverables:**
- Unity project exported as iOS XCFramework via `File > Build Settings > iOS > Export`
- Swift `GuitarExerciseViewController` embedding Unity view in bottom portion, native overlay on top
- Native settings button and back button overlaid as `UIButton` instances
- Full bridge message catalog implemented (all 15 messages in §12.2)
- `start_exercise` → exercise begins; `exercise_complete` → Swift receives result and logs it
- Android parity: Unity AAR embedded in a minimal Android Studio project with Kotlin `UnityPlayerActivity` wrapper

**Exit Criteria:**
- App launches on physical iPhone (iOS 17+) in under 3 seconds from splash
- `start_exercise` message triggers note highway within 500ms of `exercise_ready` receipt
- `exercise_complete` fires at end of 8-bar run; Swift prints score/accuracy to console
- Back button (native UIButton) triggers `exercise_exit` → Unity pauses, Swift pops VC
- No crash on 10× repeated enter/exit cycle (memory leak check via Instruments Leaks template)
- Android app runs on Pixel 6 (API 33, Vulkan) with same milestones passing

---

### Milestone 5 — Polish Pass + Microphone Input Stub (2 days)
**Goal:** Prototype is demo-ready; mic pipeline skeleton is plumbed.

**Deliverables:**
- `MicrophoneInputProvider` initialized, capturing audio, running YIN fallback pitch detection
- `CoreMLPitchDetector` stub compiling (real model slot empty, confidence always 0 → YIN used)
- HUD complete: score, multiplier badge with color ramp, accuracy slider, streak counter
- All particle systems tuned to final values from §9.4
- `S_StringVibration.shader` visually polished (envelope decay tuned to sound response)
- Red Rocks background final art assets integrated (not placeholder boxes)
- 60fps on iPhone 14 Pro (Metal) and Pixel 7 Pro (Vulkan) — verified via `Application.targetFrameRate = 60` and Xcode GPU Frame Capture showing < 8ms GPU frame time

**Exit Criteria:**
- Singing or playing guitar into mic causes notes to be detected (YIN fallback) and hit scored
- Full 8-bar demo exercise completable start-to-finish with keyboard input: score shown, "exercise_complete" received by Swift, results screen displayed natively
- Frame time budget: CPU main thread < 6ms, GPU < 8ms at 1920×1080 on iPhone 14 Pro
- Memory footprint < 280 MB RSS on device (checked via Instruments Allocations)
- 0 compiler warnings in both Unity and Xcode build

---

## 14. Technical Risks & Mitigations

### Risk 1 — Audio Latency (CRITICAL)
**Problem:** iOS / Android audio output latency (15–60ms round-trip) means the player hears their note after hitting it, causing desync confusion.

**Mitigation:**
- Use `AudioSettings.outputSampleRate` and `AudioSettings.dspTime` exclusively; never `Time.time` for timing.
- Set `AudioSettings.SetDSPBufferSize(256, 2)` in a pre-init script (`RuntimeInitializeOnLoadMethod`) → reduces buffer from default 1024 to 256 samples (5.8ms at 44100Hz).
- Add a **visual hit-offset calibration screen** (Milestone 4): player taps in time with a metronome; measured average tap offset is stored in `PlayerPrefs.SetFloat("InputLatencyOffsetSec", delta)` and added to all `InputNoteEvent.timestamp` values.
- iOS: request `AVAudioSession.Category.playAndRecord` with `options: .lowLatency` — reduces roundtrip from ~60ms to ~15ms on modern iPhones.

### Risk 2 — CoreML Model Integration (HIGH)
**Problem:** The user's CNN pitch detector may have different input shape, sample rate, or output format than stubbed.

**Mitigation:**
- Define a strict interface contract in `CoreMLPitchDetector.cs`: input = `float[512]` @ 44100Hz, output = `(int midiNote, float confidence)`. Document this in `Plugins/iOS/README_COREML.md`.
- Keep `YINFallback.cs` as a always-compilable fallback — exercise remains playable without the CNN.
- The `.mlpackage` slot in `StreamingAssets/CoreML/` is a placeholder directory; the model is swapped in without code changes.

### Risk 3 — Unity as a Library Memory Overhead (HIGH)
**Problem:** UaaL (Unity as a Library) keeps the Unity runtime alive in memory even when its view is hidden. On low-RAM devices, this can trigger iOS memory warnings.

**Mitigation:**
- Call `UnityFramework.getInstance().pause()` when Swift hides the view; this stops Unity's render loop but keeps scene state.
- Use `Addressables` for all large textures (fretboard PBR, background layers) with `autoReleaseHandle = true` — allows textures to be unloaded when guitar scene is not active.
- Set `QualitySettings.globalTextureMipmapLimit = 1` (half-res) on devices with < 3GB RAM (detected via `SystemInfo.systemMemorySize`).
- Target RSS < 280MB (§13, Milestone 5). Exceed threshold: drop background layer count from 3 → 1.

### Risk 4 — Note Highway Z-Fighting at High Fret Density (MEDIUM)
**Problem:** Notes on adjacent strings at identical Z positions may Z-fight against fretboard surface.

**Mitigation:**
- Note capsules have Y offset of `+0.02f` above string surface — but at oblique camera angle, depth buffer precision can still cause flicker.
- Set `ZTest = LEqual` and `ZWrite Off` on `M_NoteCapsule` material; capsules are rendered in a separate `RenderObjects` pass after opaque geometry.
- Add `Camera.depthTextureMode = DepthTextureMode.None` — not needed for this pass, reduces bandwidth.
- URP: capsule renderer uses `RenderQueue = 2600` (Transparent+100) which renders after all opaque.

### Risk 5 — DSP Clock Drift Over Long Sessions (MEDIUM)
**Problem:** `AudioSettings.dspTime` can accumulate drift relative to wall clock over 30+ minute sessions due to audio thread scheduling jitter on Android.

**Mitigation:**
- In `SongClock`, maintain a secondary `System.Diagnostics.Stopwatch` running in parallel.
- Every 30 seconds, compute `drift = Math.Abs(dspElapsed - stopwatchElapsed)`. If `drift > 0.050` (50ms), log warning and **resync**: recalculate `_dspStartTime` to bring clocks back into alignment.
- On Android specifically, set `AudioManager.setMode(MODE_IN_COMMUNICATION)` via Java plugin to request lower-latency scheduling.

### Risk 6 — Procedural Mesh UV Seams on Fretboard (LOW)
**Problem:** The tapered fretboard quad strip may exhibit UV stretching at the body-join end (fret 12+), making PBR textures look warped.

**Mitigation:**
- Use a **triplanar UV projection** option in `S_FretboardPBR.shader` (shader keyword `_TRIPLANAR_UV`): `uv = worldPos.xz / textureTileSize`. Eliminates all UV seam issues at cost of ~2% GPU overhead.
- For prototype: standard planar UVs are acceptable; note the triplanar option for production.

### Risk 7 — Android Vulkan + URP Shader Compatibility (MEDIUM)
**Problem:** Custom shaders (`S_StringVibration`, `S_NoteGlow`) may not compile for Vulkan SPIR-V correctly if they use unsupported HLSL intrinsics.

**Mitigation:**
- Avoid `tex2Dlod` and `ddx/ddy` in vertex shaders (not available in all Vulkan drivers).
- Use `#pragma target 3.5` (not 4.0) to stay within Vulkan feature tier universally supported on Android API 26+.
- Add Android-specific CI build step (GitHub Actions with `BuildPlayerOptions.target = BuildTarget.Android`) that runs after every commit to catch shader compile errors early.

### Risk 8 — String Lane Hit Ambiguity with Microphone Input (LOW-MEDIUM)
**Problem:** Mic/CNN input returns only MIDI note (pitch), not which physical string was played. On guitar, the same MIDI note can be played on multiple strings (e.g. E4 = string 1 fret 7 OR string 2 fret 2).

**Mitigation:**
- Tab generator's `RandomTabGenerator.PickFret()` already minimizes cross-string duplicate pitches by constraining fret range to `[0, maxFret=7]` — most notes in first position are unique per pitch.
- For ambiguous cases: `TimingEngine.OnNoteInput()` iterates all active notes matching `midiNote` (not string+fret) and hits the one with the smallest `|delta|`.
- Future: timbre analysis layer (string-specific overtone profile) plugged into CNN second output head.

---

## Appendix A — Key Constants Reference

```csharp
// ── Camera ────────────────────────────────
const float CAMERA_ARM_Y           = 1.8f;
const float CAMERA_ARM_Z           = 2.4f;
const float CAMERA_TILT_DEG        = -22f;
const float CAMERA_FOV             = 55f;

// ── Highway ───────────────────────────────
const float HIGHWAY_VISIBLE_SEC    = 2.5f;
const float HIGHWAY_VISIBLE_UNITS  = 10.0f;
const float SCROLL_SPEED           = 4.0f;      // units/sec
const float SPAWN_LOOKAHEAD_SEC    = 2.7f;
const float HIT_ZONE_Z             = 1.37f;     // fretZ[2]

// ── Timing Windows ────────────────────────
const double PERFECT_MS            = 45.0;      // ±ms
const double GOOD_MS               = 90.0;
const double LATE_MS               = 135.0;

// ── Scoring ───────────────────────────────
const int    BASE_PERFECT          = 100;
const int    BASE_GOOD             = 60;
const int    BASE_LATE             = 30;
const int[]  STREAK_THRESHOLDS     = { 0, 10, 20, 30 };  // → ×1, ×2, ×3, ×4

// ── Fretboard Geometry ────────────────────
const float FRETBOARD_LENGTH       = 12.0f;
const float FRETBOARD_NUT_WIDTH    = 0.38f;
const float FRETBOARD_BODY_WIDTH   = 0.44f;
const float FRETBOARD_THICKNESS    = 0.04f;
const int   FRET_COUNT             = 15;

// ── Object Pool ───────────────────────────
const int    NOTE_POOL_SIZE        = 64;

// ── Audio ─────────────────────────────────
const int    DSP_BUFFER_SIZE       = 256;       // samples
const int    MIC_SAMPLE_RATE       = 44100;
const int    MIC_BUFFER_SAMPLES    = 4096;
const int    MIC_HOP_SAMPLES       = 512;
const float  CNN_CONFIDENCE_THRESH = 0.72f;
const float  MIC_RMS_GATE          = 0.01f;

// ── Memory Targets ────────────────────────
const int    MAX_RSS_MB            = 280;
const int    MAX_GPU_FRAME_MS      = 8;
const int    MAX_CPU_FRAME_MS      = 6;
```

---

## Appendix B — Third-Party Libraries

| Library | Version | Purpose | License |
|---|---|---|---|
| DOTween Pro | 1.2.765 | UI/feedback animations | Commercial |
| TextMeshPro | (UPM built-in) | All text rendering | Unity Package |
| Unity Input System | 1.7.x | Supplemental gamepad support | MIT |
| Newtonsoft.Json | 13.0.3 (Unity) | Complex JSON serialization | MIT |
| UniTask | 2.5.x | Async/await without allocation | MIT |

---

*End of PLAN.md — v0.1.0*
