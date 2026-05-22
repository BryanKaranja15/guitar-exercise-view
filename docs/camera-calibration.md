# Camera Calibration — Guitar Exercise View

> Working document. All math shown. Final recommended values at bottom.
> Last updated: 2025-05-21

---

## Scene Setup

| Parameter | Value |
|---|---|
| Camera position | (0, 1.8, -2.4) |
| Camera rotation | Euler(22, 0, 0) — 22° downward tilt |
| FOV (vertical) | 55° |
| Aspect ratio | 16:9 (landscape) |
| Fretboard length | 12.0u (Z axis: nut=0, body=12) |
| Fretboard width | 0.38u (nut) → 0.45u (body) |
| Hit zone | Z = 2.4 (20% from nut) |
| Note spawn point | Z = 12.0 |
| Scroll speed (initial) | 4.0 u/s |
| String Y offset | 0.012u above fretboard surface |
| String X positions | [-0.190, -0.114, -0.038, +0.038, +0.114, +0.190] |

---

## Result 1 — Distance

Camera at (0, 1.8, -2.4). Fretboard plane at Y=0.

- **Perpendicular distance** (cam Y to fretboard): **1.80u**
- **Euclidean distance** to hit zone (0, 0, 2.4): **5.13u**

---

## Result 2 — Visible Height at Fretboard Plane

```
visibleHeight = 2 × 1.80 × tan(27.5°) = 1.87u
```

The camera sees 1.87 world units of height at the fretboard's Y=0 plane.

---

## Result 3 — Horizontal FOV and String Width

```
horizontalFOV = 2 × atan(tan(27.5°) × 1.778) = 85.6°
visibleWidth   = 2 × 1.80 × tan(42.8°) = 3.33u
```

String span (0.45u) occupies **13.5% of frame width** — comfortable, not cramped. ✅

---

## Result 4 — Fretboard Z Range Visible (Key Finding)

Camera look angle below horizontal: **atan(1.8 / 4.8) = 20.6°**
Half FOV: **27.5°**

Since 20.6° < 27.5°, the **top of the frame points 6.9° ABOVE horizontal** — it looks at the sky, not the fretboard. The top ray never intersects Y=0.

The **bottom of the frame points 48.1° below horizontal**, hitting Y=0 at Z ≈ -0.3u (just in front of the nut).

**Conclusion: the entire fretboard (Z=0 → Z=12) is visible within the lower portion of the frame.** The fretboard doesn't "end" within the FOV — it recedes to a vanishing point somewhere in the upper portion of the frame. This is correct Guitar Hero / note highway behaviour.

---

## Result 5 — Hit Zone Screen Position

Hit zone at Z=2.4. Visible fretboard starts near Z=0 at the bottom of the frame.

```
hitZone % from bottom = (2.4 / visible_z_range) × 100
```

Given the full Z=0→12 fretboard is visible, the hit zone at Z=2.4 sits at approximately **20% up from the bottom of the fretboard view** — which maps to the lower portion of the allocated highway region. ✅

---

## Result 6 — String Lane Pixel Spacing on 1080p

```
String world spacing = 0.38u / 5 gaps = 0.076u
Pixels per world unit = 1080 / 1.87 = 577px/u
String pixel spacing  = 0.076 × 577 = 43.8px
```

Minimum readable threshold: **40px**. Current value: **43.8px** — barely passes. ✅ (Marginal)

### ⚠️ Note on string spacing
At 43.8px spacing, note pills need careful sizing. If pills are too wide they'll overlap adjacent strings. Recommended pill width: **max 36px** (leaving 4px gap each side).

---

## Result 7 — Note Lookahead (Critical Issue)

The camera sees from Z≈0 (nut, bottom of frame) to Z=12 (body, vanishing point near top of allocated region). Lookahead distance from hit zone to spawn:

```
lookahead distance = 12.0 - 2.4 = 9.6u
lookahead time     = 9.6 / 4.0  = 2.4 seconds
```

**2.4 seconds is not enough.** Yousician shows approximately **5–6 seconds** of lookahead. Players need time to read ahead, especially for chord transitions.

### Fix: Adjust scroll speed, not camera

The camera already sees the full 9.6u ahead of the hit zone. The fix is simply slowing down scroll speed:

```
For 5.0s lookahead: speed = 9.6 / 5.0 = 1.92 u/s
For 6.0s lookahead: speed = 9.6 / 6.0 = 1.60 u/s
```

**Recommended scroll speed: 2.0 u/s** (clean number, gives 4.8 seconds — close to Yousician's feel).

> Note: scroll speed should be exposed as a difficulty/accessibility setting. Beginners may want 1.5 u/s (6.4s), advanced players 2.5 u/s (3.8s).

---

## Result 8 — Proposed Camera Adjustments

Current values are **geometrically sound** with one fix needed:

| Parameter | Original | Recommended | Reason |
|---|---|---|---|
| Camera position | (0, 1.8, -2.4) | **(0, 1.8, -2.4)** | No change needed |
| Camera rotation | Euler(22, 0, 0) | **Euler(22, 0, 0)** | No change needed |
| FOV | 55° | **52°** | Slight tighten: reduces visible width from 3.33u→3.1u, increases string spacing from 43.8px→47px (more breathing room) |
| Scroll speed | 4.0 u/s | **2.0 u/s** | 4.8s lookahead — matches Yousician feel |
| Note pill max width | — | **36px on 1080p** | Prevents overlap at 43-47px string spacing |

### FOV 52° string spacing recalculation:
```
visibleHeight (FOV 52°) = 2 × 1.80 × tan(26°) = 1.76u
string spacing px       = (0.076 / 1.76) × 1080 = 46.6px  ✅ Better headroom
```

---

## Final Recommended Values

```csharp
// CameraRig.cs default values
cameraPosition = new Vector3(0f, 1.8f, -2.4f);
cameraRotation = new Vector3(22f, 0f, 0f);      // Euler angles
fieldOfView    = 52f;

// NoteHighwaySystem — scroll speed
public float scrollSpeed = 2.0f;  // u/s — 4.8s lookahead
public float scrollSpeedMin = 1.5f;  // beginner (~6.4s)
public float scrollSpeedMax = 3.0f;  // advanced (~3.2s)
```

---

## Remaining Known Issue

**String spacing is marginal (46–47px at 1080p).** On smaller screens (iPhone SE = 750×1334 in landscape = 667px height for the fretboard region), string spacing will be much tighter. This needs device-specific testing during Milestone 2.

Mitigation option: scale the fretboard slightly wider (fretboardWidth × 1.2) on small screens via a responsive layout manager.

---

## Sources
- Math derived from first principles (Unity camera projection formulas)
- Yousician lookahead estimate (~5–6 seconds) from: visual analysis of `assets.yousician.com` gameplay screenshot + community gameplay video reports
- Date: 2025-05-21
