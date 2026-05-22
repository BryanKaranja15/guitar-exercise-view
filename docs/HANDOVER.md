# Hermes Session Handover — Bryan Karanja (BryanKaranja15)
> Generated: 2026-05-21 | Reason: Switching from API credits to subscription API

---

## 1. User Profile

- **Name:** Bryan Karanja | **GitHub:** BryanKaranja15
- **Primary device:** iPhone (Safari) — always needs live URLs, not file attachments
- **Design taste:** Dark/elegant, modern animations, shadcn/ui-inspired, production-quality — not rough mockups
- **Workflow preference:** Plan with Opus first, then execute. Log sources for every key fact.
- **Git:** Credentials configured on this machine. Default: public repos, push to main, enable GitHub Pages for web projects.
- **All projects pushed to:** github.com/BryanKaranja15

---

## 2. Projects Built This Session

### Project A — FastFashion Web Prototype
- **Repo:** github.com/BryanKaranja15/fastfashion
- **What it is:** Single-file HTML/React/Tailwind fashion discovery app. User describes an outfit → loading animation → results page with 5 swipeable carousels (one per clothing article: tops, bottoms, shoes, bags, jewelry). One item shown at a time per carousel. Unsplash images for product mockups.
- **Status:** ✅ Complete and deployed
- **Live URL (temporary Cloudflare tunnel):** Was active during session — tunnel will have expired. Re-deploy with: `cd /tmp/fastfashion && python3 -m http.server 8899` + new cloudflared tunnel, OR enable GitHub Pages on the repo (index.html is already there)
- **GitHub Pages URL (once enabled):** https://bryankaranja15.github.io/fastfashion
- **File location on server:** `/tmp/fastfashion/FastFashion.html` and `/tmp/fastfashion/index.html`
- **Stack:** Vanilla React (CDN), Tailwind (CDN Play), Cormorant Garamond + Inter fonts, Unsplash images
- **3 pre-loaded outfits:** Parisian Chic Brunch, NYC Streetwear Fall, Resort Vacation Look

---

### Project B — Guitar Exercise View (Unity + Swift)
- **Repo:** github.com/BryanKaranja15/guitar-exercise-view
- **What it is:** A Yousician-style guitar exercise prototype. Unity (C#) for the gameplay scene, Swift for the app shell — same architecture as Yousician. Landscape only.
- **Status:** 🟡 Milestone 1 in progress — Unity project set up, compilation errors being resolved
- **Local clone on server:** `/tmp/guitar-exercise-view/`

#### Key Architecture Decisions (DO NOT change these without Bryan's input)
1. **Option C hybrid** — True 3D guitar fretboard model IS the note highway. Camera at slight downward angle so strings read as horizontal lanes. Notes (pill-shaped with fret numbers) travel right → left.
2. **No fret wire geometry** — fretboard surface texture/inlays only. String lanes + note pills carry all gameplay info. Held notes and bends are elongated pills, not snapped to fret positions.
3. **Unity handles:** 3D scene, note highway, timing engine, scoring, feedback visuals, audio/MIDI/CNN input pipeline
4. **Swift handles:** App shell, navigation, onboarding, subscriptions, settings — NOT the gameplay
5. **UaaL lifecycle:** Unity initialized ONCE per app session, never unloaded. Swift hides/shows the Unity view (never push/pop). Scene reset happens via Unity-internal messages.
6. **World-space Canvas** for note pills — Canvas coplanar with fretboard (`Euler(-90,0,0)`), not billboarded. Pills lie flat on strings, camera angle is coplanar so text reads naturally.
7. **Namespace:** `GuitarExerciseView.CameraSystem` (NOT `.Camera` — shadows UnityEngine.Camera)

#### Unity Project Details
- **Unity version:** 2022.3.62f3 (LTS, released Oct 2025, verified no known CVEs)
- **Render pipeline:** URP 14.0.9
- **Camera:** position `(0, 1.8, -2.4)`, rotation `Euler(22, 0, 0)`, FOV `52°`
- **Scroll speed:** `2.0 u/s` → gives 4.8s lookahead (Yousician parity)
- **String positions (X):** `[-0.190, -0.114, -0.038, +0.038, +0.114, +0.190]`
- **Hit zone:** Z = 2.4 (20% from nut)
- **Note spawn:** Z = 12.0

#### Scripts written (in repo)
| File | Purpose | Status |
|---|---|---|
| `Assets/Scripts/Fretboard/FretboardMesh.cs` | Procedural fretboard mesh (22 frets, radius, inlays) | ⚠️ Exists but Bryan is moving to a 3D model instead — may be deprecated |
| `Assets/Scripts/Fretboard/StringRenderer.cs` | 6 LineRenderer strings with gauge-accurate widths | ✅ Clean |
| `Assets/Scripts/Camera/CameraRig.cs` | Camera positioning, `[ExecuteInEditMode]` | ✅ Clean (namespace: GuitarExerciseView.CameraSystem) |
| `Assets/Scripts/Editor/MilestoneOneSetup.cs` | Editor menu to scaffold the scene | ⚠️ Had bugs — see Compilation Issues below |
| `Assets/Scripts/Notes/NoteVisual.cs` | Note pill component on world-space canvas | ✅ Written (Milestone 2 code, shouldn't cause M1 errors) |

#### 3D Model Decision (IMPORTANT — last thing discussed)
Bryan decided to **use a pre-made 3D model instead of procedural mesh**. He imported the **Low Poly Guitars Pack** (Unity Asset Store, free, by Studio Nik) into his Unity project. Key details:
- Electric guitar: **460 vertices** (ultra lightweight)
- *"Strings are separate parts and can be deactivated"* — we'll deactivate default strings and replace with LineRenderers
- **Pack is already imported** in Bryan's Unity project under `Assets/Studio Nik/Low Poly Guitars Pack/`
- **Next step:** Find the exact prefab path inside that folder, then write a new simple `MilestoneOneSetup.cs` that: (1) places the electric guitar prefab, (2) positions camera looking down the neck, (3) adds 6 LineRenderer strings on top of the fretboard, nothing else

#### Compilation Issues Encountered & Fixed
| Issue | Cause | Fix Applied |
|---|---|---|
| `com.unity.modules.ios` error | Module needs iOS Build Support installed | Removed from manifest.json |
| `Camera` ambiguous reference in NoteVisual.cs | Namespace `GuitarExerciseView.Camera` shadowed `UnityEngine.Camera` | Renamed namespace to `GuitarExerciseView.CameraSystem` |
| Pink/magenta background | Broken shader fallback + rotation `(90,0,0)` | Fixed shader lookup + rotation set to `(0,0,0)` |
| Fretboard invisible in Scene view | No `[ExecuteInEditMode]` | Added to FretboardMesh.cs and CameraRig.cs |
| Duplicate method block in MilestoneOneSetup.cs | Bad patch created double method | Fixed via Python line deletion |

#### Open Issues (tracked in `docs/open-questions.md`)
| # | Issue | Status |
|---|---|---|
| 2 | CNN model assumptions (framework? input? latency?) | 🔴 Open — needs Bryan's input |
| 4 | Hit zone appearance in 3D | 🔴 Open — design not yet resolved |
| 5 | Android parity (Kotlin/Java bridge) | 🔴 Open — iOS only so far |
| 6 | Guitar Pro import complexity (AlphaTab C# gaps) | 🔴 Open — Milestone 3 concern |
| 8 | Android performance budget (60fps on mid-range) | 🔴 Open |

#### Milestones
| # | Goal | Status |
|---|---|---|
| 1 | Fretboard renders, 6 strings visible, correct camera angle | 🟡 In progress |
| 2 | Note highway: pills spawn at Z=12, scroll to Z=0, hit zone | ⬜ Not started |
| 3 | Tab/song data system + random tab generator | ⬜ Not started |
| 4 | Input pipeline (keyboard → CNN stub → MIDI stub) | ⬜ Not started |
| 5 | Scoring, feedback (Perfect!/particles), HUD overlay | ⬜ Not started |

#### Key Documentation in Repo
- `PLAN.md` — 2000-line full technical prototype plan
- `docs/sources.md` — all research sources + architecture decisions logged with dates
- `docs/open-questions.md` — all unresolved issues with status
- `docs/camera-calibration.md` — camera math worked out, final values justified
- `README_MILESTONE1.md` — step-by-step verification guide for Milestone 1

---

## 3. Standing Instructions

1. **Source logging:** Every key project fact must be logged in `docs/sources.md` with source URL and date. This was explicitly requested by Bryan.
2. **Security:** Always verify version numbers have no known CVEs before specifying them. Never use versions without checking.
3. **Namespace rule:** Never use Unity reserved words or engine class names (`Camera`, `Input`, `Physics`, `UI`, `Audio`) as C# namespace segments.
4. **Debugging:** `[ExecuteInEditMode]` should be on all scene-facing MonoBehaviours so Bryan can see results in Scene view without pressing Play.
5. **Live URLs:** Bryan is on iPhone. Always deploy to a live URL (GitHub Pages, Cloudflare tunnel) rather than sending files.
6. **Git workflow:** All work pushed to GitHub. Bryan does `git pull` on his Mac. Commit messages use emoji prefixes (🎸 feature, 🔧 fix, 🔒 security, 📐 design, 📋 docs).
7. **3D model direction:** Do NOT go back to procedural mesh for the fretboard. Bryan has the Low Poly Guitars Pack imported. Build on top of that model.

---

## 4. Immediate Next Action

**Where the conversation ended:**
Bryan had just sent a screenshot showing the Low Poly Guitars Pack was successfully imported into Unity (`Assets/Studio Nik/Low Poly Guitars Pack/`). The next message asked him to screenshot the folder contents so we could find the exact prefab path.

**What to do when Bryan comes back:**
1. Ask him to share the Project panel screenshot showing `Assets/Studio Nik/Low Poly Guitars Pack/` folder contents (or just the prefab path)
2. Once you have the path, write a clean minimal `MilestoneOneSetup.cs` that:
   - Instantiates the electric guitar prefab at origin
   - Deactivates the default strings on the model
   - Adds 6 LineRenderer strings positioned on the fretboard neck
   - Places camera at `(0, 1.8, -2.4)` with `Euler(22, 0, 0)`, FOV 52°
   - Adds key light + fill light
   - NO procedural mesh, NO material system, NO editor tooling beyond what's needed
3. Push to GitHub, Bryan pulls, scene should render immediately in Scene view

---

## 5. Server State

- `/tmp/fastfashion/` — FastFashion HTML prototype (intact)
- `/tmp/guitar-exercise-view/` — Guitar project local clone (intact, in sync with GitHub)
- Git credentials configured globally for BryanKaranja15
- Python HTTP server and Cloudflare tunnel from earlier session are likely dead — restart if needed
