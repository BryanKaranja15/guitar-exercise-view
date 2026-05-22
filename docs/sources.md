# Project Sources & Research Log

> All key facts, design decisions, and technical references for the guitar exercise view prototype.
> Updated as research progresses. Format: fact → source → date logged.

---

## Yousician UI Research

| Fact | Source | Date |
|---|---|---|
| Gameplay screenshot used for visual analysis — layout, note colors, HUD structure, hit zone | `https://assets.yousician.com/app/uploads/2025/04/15014125/mgpm-screen-black-yousician.png` | 2025-05-21 |
| Feature list, microphone-based pitch detection, landscape layout, iPad primary canvas | `https://yousician.com/guitar` | 2025-05-21 |
| Unity used for gameplay view, native iOS/Android shell for subscriptions/onboarding | Unity case study (now 404) + corroborated by app architecture analysis | 2025-05-21 |
| Note pills are capsule-shaped with fret numbers inside, color-coded by string group | Visual analysis of official screenshot | 2025-05-21 |
| Hit zone = two gold horizontal lines ~20–25% from left edge, white cursor dot | Visual analysis of official screenshot | 2025-05-21 |
| Progress bar at bottom = note density map (green played, yellow upcoming, red dense) | Visual analysis of official screenshot | 2025-05-21 |
| HUD: score top-right, multiplier badge top-left, section label top-center | Visual analysis of official screenshot | 2025-05-21 |
| "Perfect!" feedback text appears above highway near hit zone, white sparkle particles | Visual analysis of official screenshot | 2025-05-21 |
| Background is near-black #0A0A0A with subtle perspective grid floor lines, no 3D scene | Visual analysis of official screenshot | 2025-05-21 |
| Strum direction shown as dotted arc lines above hit zone (up/down strumming guide) | Visual analysis of official screenshot | 2025-05-21 |
| No public GDC talks or engineering blog posts found | GDC Vault search, medium.com/yousician-engineering (blocked), engineering.yousician.com (404) | 2025-05-21 |

---

## Note Color System (Yousician Reference)

| String Group | Color | Hex |
|---|---|---|
| Strings 1–2 (e, B) | Bright lime green | `~#39FF14` |
| Strings 3–4 (G, D) | Orange | `~#FF9800` |
| Strings 5–6 (A, E) | Purple/Magenta | `~#E040FB` |
| Open strings (fret 0) | Gray | `~#9E9E9E` |

---

## Architecture Decisions

| Decision | Rationale | Date |
|---|---|---|
| Unity for entire gameplay scene (fretboard, notes, timing, input, scoring, feedback) | Same pattern as Yousician; Unity owns real-time 3D + game loop | 2025-05-21 |
| Swift/native for app shell (navigation, onboarding, subscriptions, settings, library) | Modern iOS animations, App Store compliance, same pattern as Yousician | 2025-05-21 |
| Option C hybrid: true 3D fretboard IS the highway, camera angle creates lane perspective | Preserves highway readability while enabling full guitar customization (inlays, wood, color) | 2025-05-21 |
| Landscape-only layout | Guitar exercise requires wide viewport for note lookahead; matches Yousician primary design canvas | 2025-05-21 |
| 6 strings (guitar only) to start | Scope control for prototype; bass/ukulele added later | 2025-05-21 |
| Static Red Rocks parallax background first, 3D environment later | Scope control; parallax gives depth without full 3D scene cost | 2025-05-21 |
| CNN model (in training) + MIDI interface for input pipeline | User is training CNN for real-time note detection; MIDI for latency-critical pro use | 2025-05-21 |
| Random tab generator for prototype (no Guitar Pro import yet) | Scope control; GP import added after core highway is proven | 2025-05-21 |
| No fret wire geometry rendered (Option C) — fretboard surface texture and inlays only | Option A windowed fretboard rejected: held notes and bends span multiple fret positions, windowed scroll would clip sustains mid-travel. String lanes + note pills carry all gameplay info. | 2025-05-21 |
| Issue #3 — Note pill orientation: **Approach B chosen** — World-space Canvas coplanar with fretboard (rotated Euler -90° X), 1 world unit = 100 canvas pixels, notes travel in canvas-Y (= world Z). Rejected Approach A (billboard LookAt): pills float off fretboard surface, per-frame LookAt cost on mobile, perspective skew at screen edges. Rejected Approach C (screen-space canvas): requires continuous world-to-screen projection math; breaks on any camera change; loses physical depth feel. Approach B keeps pills physically grounded on strings, text reads naturally from the -22° camera POV because camera and canvas share the same tilt plane, zero per-frame overhead. Implemented in `Assets/Scripts/Notes/NoteVisual.cs`. | Architecture analysis + implementation | 2026-05-22 |

---

## Unity Technical References

| Topic | Source | Date |
|---|---|---|
| Unity as iOS/Android plugin (UnityFramework embed) | `https://docs.unity3d.com/Manual/UnityasaLibrary-iOS.html` | — (to verify) |
| Unity ↔ Swift messaging via UnitySendMessage / native callbacks | Unity docs + community pattern | — (to verify) |

---

## Unity Compilation Lessons Learned

| Issue | Root Cause | Fix |
|---|---|---|
| `com.unity.modules.ios` error on open | Module only available when iOS Build Support installed + iOS build target active | Removed from manifest.json — Unity re-adds it automatically when switching to iOS target |
| `Camera` ambiguous reference in NoteVisual.cs | `namespace GuitarExerciseView.Camera` shadows `UnityEngine.Camera` | Renamed namespace to `GuitarExerciseView.CameraSystem` in CameraRig.cs (root fix). Also qualified `UnityEngine.Camera` in NoteVisual.cs (belt + suspenders) |
| Pink/magenta background | Broken shader fallback + rotation `(90,0,0)` | Fixed `CreateBackgroundMaterial()` with proper URP shader lookup, rotation set to `(0,0,0)` |
| Fretboard invisible in Scene view | No `[ExecuteInEditMode]` | Added to `FretboardMesh.cs` and `CameraRig.cs` |



- [ ] Unity "as a library" iOS integration — exact Xcode setup steps
- [ ] Best Unity shader approach for realistic guitar strings (LineRenderer vs custom mesh)
- [ ] Guitar Pro / GuitarJSON tab format spec for future import
- [ ] CNN model integration point — Core ML vs direct Unity microphone pipeline
- [ ] MIDI iOS interface options (AudioKit, CoreMIDI, USB-C adapter support)
- [ ] Red Rocks venue reference images for parallax background art
