# Open Questions & Unresolved Issues

> Tracked issues raised during planning/build that need resolution before moving to the next milestone.
> Format: status, issue, resolution path.

---

## Status Legend
- 🔴 Open — not yet resolved
- 🟡 In Progress
- ✅ Resolved — see notes

---

## Issues

### #1 — UaaL Lifecycle (Unity as a Library)
**Status:** ✅ Resolved (design decision documented)
**Summary:** Unity can only be initialized once per app session. The Swift layer must keep Unity resident in memory — hide/show the view, never unload. Scene reset happens inside Unity via messages, not scene reload. Swift uses a persistent root UIViewController to host Unity.
**Logged:** `docs/sources.md` — Architecture Decisions

---

### #2 — CNN Model Integration Assumptions
**Status:** 🔴 Open
**Issue:** Model framework (PyTorch/TensorFlow → Core ML), input format (raw audio / mel spectrogram), output latency on device all unknown. Real-time guitar feedback needs <30ms end-to-end. A heavy CNN may not hit that on device.
**Needs:** Bryan to answer: framework? input format? estimated model size/latency?

---

### #3 — Note Pill Orientation on Tilted Fretboard
**Status:** ✅ Resolved
**Summary:** World-space Canvas coplanar with fretboard surface (`Euler(-90,0,0)`). Pills lie flat on strings, camera angle is coplanar so text reads naturally. No per-frame LookAt math.
**File:** `Assets/Scripts/Notes/NoteVisual.cs`
**Logged:** `docs/sources.md` — Architecture Decisions

---

### #4 — Hit Zone Appearance in 3D
**Status:** 🔴 Open
**Issue:** On a tilted 3D fretboard, a screen-vertical hit zone line is angled in world space. Getting it to appear as a clean consistent vertical marker across all 6 strings at different Z depths needs a specific approach — not yet designed or implemented.
**Needs:** Design decision + implementation in next milestone pass.

---

### #5 — Android Parity
**Status:** 🔴 Open
**Issue:** The Unity ↔ Swift bridge in PLAN.md is iOS-only (ObjC/Swift). Android needs a separate Kotlin/Java equivalent with a different message-passing pattern (JNI / UnityPlayer.UnitySendMessage).
**Needs:** Android bridge design added to PLAN.md before Milestone 2.

---

### #6 — Guitar Pro Import Complexity
**Status:** 🔴 Open
**Issue:** `.gp`/`.gpx` is a complex binary format. The main C# library (AlphaTab) is JavaScript-first — the C# port has gaps. Needs a proper decision before the tab system is built beyond the random generator.
**Options:** AlphaTab C# port / custom parser / server-side conversion / JS bridge.
**Needs:** Decision before Milestone 3 (tab system milestone).

---

### #7 — Camera Angle Values Unverified
**Status:** ✅ Resolved
**Summary:** Camera position (0,1.8,-2.4) and rotation Euler(22,0,0) confirmed correct. FOV adjusted 55°→52° for better string spacing (46.6px vs 43.8px on 1080p). Critical fix: scroll speed reduced 4.0→2.0 u/s for 4.8s lookahead (Yousician parity). Full math in `docs/camera-calibration.md`.

---

### #8 — Performance Budget on Android
**Status:** 🔴 Open
**Issue:** 6 LineRenderers + procedural mesh + particles + parallax + URP on mid-range Android (e.g. Samsung A-series) could struggle to hit 60fps. No profiling gates defined for milestones.
**Needs:** Target device spec defined, profiling gates added to milestone exit criteria.

---

## Summary

| # | Issue | Status |
|---|---|---|
| 1 | UaaL lifecycle | ✅ Resolved |
| 2 | CNN model assumptions | 🔴 Open |
| 3 | Note pill orientation | ✅ Resolved |
| 4 | Hit zone in 3D | 🔴 Open |
| 5 | Android parity | 🔴 Open |
| 6 | Guitar Pro import | 🔴 Open |
| 7 | Camera angle unverified | 🟡 In Progress |
| 8 | Performance budget Android | 🔴 Open |
