// ─────────────────────────────────────────────────────────────────────────────
//  NoteVisual.cs  —  Issue #3: Note pill orientation on a tilted 3D fretboard
//  GuitarExerciseView  •  Approach B: World-space Canvas on fretboard plane
// ─────────────────────────────────────────────────────────────────────────────
//
//  PROBLEM
//  ───────
//  The fretboard mesh lies in the world XZ plane. CameraRig pitches down at
//  -22° so the player sees the highway at a perspective angle. If note pills
//  are standard world-space quads they inherit that -22° tilt, making fret
//  numbers hard to read.
//
//  APPROACH COMPARISON
//  ───────────────────
//
//  Approach A — Billboard quads (LookAt camera each frame)
//    + Always perpendicular to camera; text is never tilted in screen space.
//    − Pills appear to float OFF the fretboard surface; at screen edges,
//      perspective distortion causes visible pop/skew.
//    − Per-frame LookAt on dozens of pooled objects adds unnecessary cost on
//      mobile; shader-based UNITY_MATRIX_V cancellation is cheaper but still
//      produces the 'floating' visual artifact.
//    − Breaks the 3-D illusion that grounds the note highway on a real guitar.
//
//  Approach C — Screen-space Canvas overlay (bottom 40 % of screen)
//    + Perfectly readable at all times; no 3-D orientation issues.
//    − Requires continuous world-to-screen projection to keep UI elements
//      aligned with the 3-D strings.  Any camera tweak (FOV, position, aspect)
//      breaks the alignment and requires recalibrating the projection math.
//    − Notes lose physical depth; parallax, held-note bends, and string
//      vibration animations become decoupled from the visual guitar.
//
//  Approach B — World-space Canvas coplanar with fretboard  ← CHOSEN
//    The Canvas is parented to FretboardRoot with rotation Euler(-90, 0, 0),
//    placing it flat in the XZ world plane at the fretboard surface height.
//    The camera looks at the scene at -22°; because the canvas and fretboard
//    share the same plane the player perceives text exactly as if it were
//    printed flat on a surface they are viewing from above — natural and
//    immediately legible.  Pills lie ON the strings, preserving the 3-D grounded
//    feel.  No per-frame matrix tricks, no projection math.
//
//    Coordinate mapping when Canvas rotation = Euler(-90, 0, 0):
//      Canvas local X  →  World X  (across string width)
//      Canvas local Y  →  World Z  (along scale length / travel direction)
//
//    Notes spawn at high canvas-Y (far end, world Z ≈ scaleLength + lookahead)
//    and travel toward low canvas-Y (hit zone, world Z ≈ hitZoneWorldZ).
//
//  CANVAS SETUP (do once in scene, not per-note)
//  ─────────────────────────────────────────────
//    1. Create an empty "NoteHighway" GameObject, child of FretboardRoot.
//    2. Add a Canvas component → Render Mode: World Space.
//    3. Set Canvas rotation to Euler(-90, 0, 0).
//    4. Position it at (0, 0.003, 0) — 3 mm above fretboard surface to prevent Z-fight.
//    5. Set RectTransform width = fretboard bodyWidth (0.45), height = travel range (~16).
//    6. Set Canvas.worldCamera = Camera.main (needed for UI events; safe to leave null
//       if no pointer input on notes).
//    7. Set Canvas referencePixelsPerUnit = 100.  One world unit = 100 canvas pixels.
//    8. Add a CanvasScaler → set UI Scale Mode to "Constant Pixel Size".
//    9. Add a GraphicRaycaster only if tap-to-retrigger is required.
//   10. Instantiate / pool NoteVisual prefabs as children of this Canvas.
//
// ─────────────────────────────────────────────────────────────────────────────

using System;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT || UNITY_2021_OR_NEWER
using TMPro;
#endif

namespace GuitarExerciseView.Notes
{
    // ─── Data ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Immutable description of a single note event passed to NoteVisual.Initialize().
    /// </summary>
    [Serializable]
    public struct NoteData
    {
        /// <summary>String index 0 (high e) … 5 (low E).</summary>
        public int stringIndex;

        /// <summary>Fret number 0 (open) … 22.</summary>
        public int fretNumber;

        /// <summary>Beat timestamp in the song (used by NoteSpawner for timing).</summary>
        public float beatTime;

        /// <summary>Duration in beats (> 0 for held / sustain notes).</summary>
        public float durationBeats;

        public NoteData(int str, int fret, float beat = 0f, float dur = 0f)
        {
            stringIndex   = str;
            fretNumber    = fret;
            beatTime      = beat;
            durationBeats = dur;
        }
    }

    // ─── NoteVisual ──────────────────────────────────────────────────────────

    /// <summary>
    /// Controls a single note-pill GameObject on the World-space Note Highway Canvas.
    /// Attach to the root of a NoteVisual prefab whose hierarchy looks like:
    /// <code>
    ///   NoteVisual (RectTransform + NoteVisual.cs)
    ///   ├── PillBackground  (RectTransform + Image)   – capsule sprite
    ///   ├── FretLabel       (RectTransform + TMP_Text) – fret number
    ///   └── SustainBar      (RectTransform + Image)   – shown for held notes
    /// </code>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class NoteVisual : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────

        [Header("Child References")]
        [SerializeField] private Image    _pillBackground;
        [SerializeField] private Image    _sustainBar;

#if TMP_PRESENT || UNITY_2021_OR_NEWER
        [SerializeField] private TMP_Text _fretLabel;
#else
        [SerializeField] private Text     _fretLabel;
#endif

        [Header("Highway Layout  (world units, maps to canvas-Y)")]
        [Tooltip("Canvas Y value where notes are spawned (world Z spawn point).")]
        [SerializeField] private float _spawnCanvasY   = 14f;

        [Tooltip("Canvas Y of the hit zone (world Z where the player presses).")]
        [SerializeField] private float _hitZoneCanvasY =  1.2f;

        [Tooltip("Canvas Y past which the note is returned to pool.")]
        [SerializeField] private float _despawnCanvasY = -0.5f;

        [Header("Movement")]
        [Tooltip("Travel speed in world units per second (canvas-Y units/s).")]
        [SerializeField] private float _travelSpeed = 4.5f;

        [Header("Pill Sizing  (canvas pixel units, 100 px = 1 world unit)")]
        [SerializeField] private float _pillWidth  = 28f;   // ≈ 0.28 wu — slightly narrower than string gap
        [SerializeField] private float _pillHeight = 22f;   // ≈ 0.22 wu — compact height

        [Header("Hit Flash")]
        [SerializeField] private float  _hitFlashDuration = 0.12f;
        [SerializeField] private Color  _hitFlashColor    = Color.white;

        // ─── Events ──────────────────────────────────────────────────────────

        /// <summary>Fired the first frame the pill crosses the hit zone threshold.</summary>
        public event Action<NoteVisual> OnReachHitZone;

        /// <summary>Fired when the pill moves past the despawn line and is deactivated.</summary>
        public event Action<NoteVisual> OnDespawned;

        // ─── Runtime state ───────────────────────────────────────────────────

        private RectTransform _rectTransform;
        private NoteData      _noteData;
        private bool          _isActive;
        private bool          _hitZoneFired;
        private bool          _hasBeenHit;
        private float         _hitFlashTimer;

        // Original color saved so we can restore after hit flash
        private Color _baseColor;

        // ─── String color palette (matches Note Color System in sources.md) ──

        private static readonly Color[] StringColors =
        {
            new Color(0.224f, 1.000f, 0.078f),  // string 0: high e  — lime   #39FF14
            new Color(0.224f, 1.000f, 0.078f),  // string 1: B       — lime
            new Color(1.000f, 0.596f, 0.000f),  // string 2: G       — orange #FF9800
            new Color(1.000f, 0.596f, 0.000f),  // string 3: D       — orange
            new Color(0.878f, 0.251f, 0.984f),  // string 4: A       — purple #E040FB
            new Color(0.878f, 0.251f, 0.984f),  // string 5: low E   — purple
        };

        private static readonly Color OpenStringColor =
            new Color(0.620f, 0.620f, 0.620f);  // fret 0 open string — grey #9E9E9E

        // ─── String X positions (world units) ────────────────────────────────
        // Mirrors the calculation in StringRenderer so pills sit on their string.
        //   nutWidth / 5 gaps for 6 strings, centered at X = 0.

        private const float NutWidth      = 0.38f;
        private const float StringSpacing = NutWidth / 5f;

        private static float StringXPosition(int stringIndex)
        {
            // Clamp to valid range
            int idx = Mathf.Clamp(stringIndex, 0, 5);
            return (-NutWidth * 0.5f) + (idx * StringSpacing);
        }

        // ─── Unity lifecycle ─────────────────────────────────────────────────

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            // Size the root RectTransform to pill dimensions
            if (_rectTransform != null)
                _rectTransform.sizeDelta = new Vector2(_pillWidth, _pillHeight);

            gameObject.SetActive(false); // pool-ready: start inactive
        }

        private void Update()
        {
            if (!_isActive) return;

            // ── Move toward player (decreasing canvas Y = decreasing world Z) ──
            Vector2 pos = _rectTransform.anchoredPosition;
            pos.y -= _travelSpeed * Time.deltaTime
                     * 100f; // convert world-units/s to canvas-pixels/s
                              // (canvas referencePixelsPerUnit = 100)
            _rectTransform.anchoredPosition = pos;

            // ── Hit-zone crossing ─────────────────────────────────────────────
            float currentWorldY = pos.y / 100f; // canvas px → world units
            if (!_hitZoneFired && currentWorldY <= _hitZoneCanvasY)
            {
                _hitZoneFired = true;
                OnReachHitZone?.Invoke(this);
            }

            // ── Despawn ───────────────────────────────────────────────────────
            if (currentWorldY <= _despawnCanvasY)
            {
                Deactivate();
                OnDespawned?.Invoke(this);
                return;
            }

            // ── Hit-flash fade ────────────────────────────────────────────────
            if (_hasBeenHit && _hitFlashTimer > 0f)
            {
                _hitFlashTimer -= Time.deltaTime;
                float t = _hitFlashTimer / _hitFlashDuration;
                SetPillColor(Color.Lerp(_baseColor, _hitFlashColor, t));
            }
        }

        // ─── Public API ──────────────────────────────────────────────────────

        /// <summary>
        /// Activates and configures the pill for a new note event.
        /// Call this instead of Instantiate — this component is poolable.
        /// </summary>
        /// <param name="data">The note to display.</param>
        /// <param name="travelSpeed">World units per second (optional override).</param>
        public void Initialize(NoteData data, float travelSpeed = -1f)
        {
            _noteData     = data;
            _isActive     = true;
            _hitZoneFired = false;
            _hasBeenHit   = false;

            if (travelSpeed > 0f)
                _travelSpeed = travelSpeed;

            // ── Position at spawn point on the correct string lane ────────────
            // Canvas local X = world X (string position)
            // Canvas local Y = world Z * 100 (pixels per unit)
            float canvasX = StringXPosition(data.stringIndex) * 100f;
            float canvasY = _spawnCanvasY * 100f;
            _rectTransform.anchoredPosition = new Vector2(canvasX, canvasY);

            // ── Appearance ────────────────────────────────────────────────────
            _baseColor = GetNoteColor(data.stringIndex, data.fretNumber);
            SetPillColor(_baseColor);

            if (_fretLabel != null)
            {
                _fretLabel.text = data.fretNumber == 0 ? "O" : data.fretNumber.ToString();
                _fretLabel.enabled = true;
            }

            // ── Sustain bar ───────────────────────────────────────────────────
            if (_sustainBar != null)
            {
                bool isSustain = data.durationBeats > 0.05f;
                _sustainBar.gameObject.SetActive(isSustain);
                if (isSustain)
                {
                    // Stretch the sustain bar behind the pill proportionally.
                    // durationBeats * beatsToUnits ≈ rough world-unit length;
                    // caller should set this via SetSustainLength() once BPM is known.
                    float barHeight = data.durationBeats * _travelSpeed * 60f; // rough px
                    var barRect = _sustainBar.rectTransform;
                    barRect.sizeDelta    = new Vector2(_pillWidth * 0.45f, barHeight);
                    barRect.anchoredPosition = new Vector2(0f, barHeight * 0.5f);
                    _sustainBar.color    = new Color(_baseColor.r, _baseColor.g,
                                                     _baseColor.b, 0.5f);
                }
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Overrides the sustain-bar length when the BPM and travel speed are known.
        /// </summary>
        /// <param name="worldUnitLength">Length of the hold in world units.</param>
        public void SetSustainLength(float worldUnitLength)
        {
            if (_sustainBar == null || !_sustainBar.gameObject.activeSelf) return;

            float px = worldUnitLength * 100f;
            var barRect = _sustainBar.rectTransform;
            barRect.sizeDelta       = new Vector2(_pillWidth * 0.45f, px);
            barRect.anchoredPosition = new Vector2(0f, px * 0.5f);
        }

        /// <summary>
        /// Triggers the hit-flash animation. Call this from the scoring system
        /// when a note is successfully matched.
        /// </summary>
        public void PlayHitEffect()
        {
            _hasBeenHit    = true;
            _hitFlashTimer = _hitFlashDuration;
        }

        /// <summary>
        /// Returns the pill's current world-Z position (reconstructed from canvas Y).
        /// Useful for the hit-detection window check.
        /// </summary>
        public float WorldZ =>
            (_rectTransform != null)
                ? _rectTransform.anchoredPosition.y / 100f
                : 0f;

        /// <summary>The note data this pill was initialized with.</summary>
        public NoteData Data => _noteData;

        /// <summary>Deactivates and resets this pill for pool reuse.</summary>
        public void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private void SetPillColor(Color c)
        {
            if (_pillBackground != null) _pillBackground.color = c;
        }

        private static Color GetNoteColor(int stringIndex, int fretNumber)
        {
            if (fretNumber == 0) return OpenStringColor;
            int idx = Mathf.Clamp(stringIndex, 0, StringColors.Length - 1);
            return StringColors[idx];
        }

        // ─── Editor gizmos ───────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Visualise spawn / hit-zone / despawn lines in Scene view when selected.
            // These are shown in the Canvas's local plane.
            var rt = GetComponent<RectTransform>();
            if (rt == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Transform ct = canvas.transform;

            // Convert canvas Y (world-unit scale) to world position on the canvas plane.
            Vector3 HitZonePt(float cY) =>
                ct.TransformPoint(new Vector3(rt.anchoredPosition.x / 100f, 0f, cY));

            Gizmos.color = new Color(1f, 0.85f, 0f, 0.8f);  // gold = hit zone
            DrawHorizontalLine(ct, _hitZoneCanvasY, 0.5f);

            Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.5f);  // green = spawn
            DrawHorizontalLine(ct, _spawnCanvasY, 0.5f);

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);  // red = despawn
            DrawHorizontalLine(ct, _despawnCanvasY, 0.5f);
        }

        private static void DrawHorizontalLine(Transform canvasTransform, float worldY, float halfWidth)
        {
            // In canvas local space: X axis is across the string width, Y axis is world Z.
            // Canvas rotation Euler(-90,0,0) maps local (x, y, 0) → world (x, 0, y).
            Vector3 left  = canvasTransform.TransformPoint(new Vector3(-halfWidth, 0f, worldY));
            Vector3 right = canvasTransform.TransformPoint(new Vector3( halfWidth, 0f, worldY));
            Gizmos.DrawLine(left, right);
        }
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  NoteHighwayCanvas  (companion component — attach to the Canvas GameObject)
    // ─────────────────────────────────────────────────────────────────────────
    //
    //  Bootstraps and owns the World-space Canvas that serves as the note highway.
    //  This is intentionally a separate, lightweight component so NoteVisual can
    //  remain a pure "pill view" class with no scene-construction responsibilities.
    //
    /// <summary>
    /// Bootstraps the World-space Note Highway Canvas.
    /// Attach to an empty child GameObject of FretboardRoot alongside a Canvas component.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class NoteHighwayCanvas : MonoBehaviour
    {
        [Header("Fretboard reference dimensions  (must match FretboardMesh)")]
        [SerializeField] private float _scaleLength = 12f;
        [SerializeField] private float _boardWidth  = 0.45f;  // bodyWidth

        [Header("Canvas Y travel range")]
        [SerializeField] private float _lookaheadUnits = 4f;  // extra ahead of spawn

        [Header("Surface offset")]
        [SerializeField] private float _yAboveSurface = 0.003f;

        private Canvas         _canvas;
        private RectTransform  _canvasRect;

        private void Awake()
        {
            _canvas     = GetComponent<Canvas>();
            _canvasRect = GetComponent<RectTransform>();

            // Render mode: World Space so the canvas sits in 3D.
            _canvas.renderMode = RenderMode.WorldSpace;

            // Assign main camera for UI events (safe if Camera.main is null at Awake;
            // NoteSpawner can call AssignCamera() once the camera is ready).
            if (UnityEngine.Camera.main != null)
                _canvas.worldCamera = UnityEngine.Camera.main;

            // Lie flat on the fretboard: rotate the canvas so its local XY plane
            // aligns with the world XZ plane.  After this rotation:
            //   canvas local X  →  world X
            //   canvas local Y  →  world Z
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            // Position just above the fretboard surface.
            transform.localPosition = new Vector3(0f, _yAboveSurface, 0f);

            // Size the canvas rect in world units (1 unit = 100 px).
            float totalHeight = _scaleLength + _lookaheadUnits;
            _canvasRect.sizeDelta = new Vector2(
                _boardWidth  * 100f,   // canvas pixel width
                totalHeight  * 100f    // canvas pixel height
            );

            // Anchor the rect so (0,0) is at the nut (world Z = 0).
            // With pivot (0.5, 0), X is centered and Y starts at nut.
            _canvasRect.pivot       = new Vector2(0.5f, 0f);
            _canvasRect.anchoredPosition3D = Vector3.zero;

            // Pixels-per-unit: 100 canvas pixels = 1 world unit.
            _canvas.referencePixelsPerUnit = 100f;
        }

        /// <summary>Assign the world camera after scene load if Camera.main was null at Awake.</summary>
        public void AssignCamera(UnityEngine.Camera cam) => _canvas.worldCamera = cam;

        /// <summary>The scale factor to convert world units to canvas pixels (always 100).</summary>
        public const float PixelsPerUnit = 100f;
    }
}
