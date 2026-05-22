#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using GuitarExerciseView.Fretboard;
using GuitarExerciseView.CameraSystem;

namespace GuitarExerciseView.Editor
{
    /// <summary>
    /// Unity Editor tooling for the Guitar Exercise View project.
    /// Provides menu items to scaffold the Milestone 1 scene and verify camera setup.
    /// </summary>
    public static class MilestoneOneSetup
    {
        // ─────────────────────────────────────────────────────────────────────────
        // Menu: GuitarExercise/Setup/Create Milestone 1 Scene
        // ─────────────────────────────────────────────────────────────────────────

        [MenuItem("GuitarExercise/Setup/Create Milestone 1 Scene")]
        public static void CreateMilestoneOneScene()
        {
            // Prompt save if the current scene has unsaved changes
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[MilestoneOneSetup] Scene creation cancelled by user.");
                return;
            }

            // 1. Create a fresh scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── 2. FretboardRoot ─────────────────────────────────────────────────
            GameObject fretboardRoot = new GameObject("FretboardRoot");
            fretboardRoot.transform.position = Vector3.zero;
            fretboardRoot.transform.rotation = Quaternion.identity;

            FretboardMesh fretboardMesh = fretboardRoot.AddComponent<FretboardMesh>();
            // Default values are set in the class definition; call awake-equivalent via reflection is not safe here.
            // Values are set via SerializedObject to properly dirty the object.
            SerializedObject soFretboard = new SerializedObject(fretboardMesh);
            soFretboard.FindProperty("numFrets").intValue       = 22;
            soFretboard.FindProperty("scaleLength").floatValue  = 12f;
            soFretboard.FindProperty("nutWidth").floatValue     = 0.38f;
            soFretboard.FindProperty("bodyWidth").floatValue    = 0.45f;
            soFretboard.FindProperty("showGizmos").boolValue    = true;
            soFretboard.ApplyModifiedProperties();

            // ── 3. StringsRoot ───────────────────────────────────────────────────
            GameObject stringsRoot = new GameObject("StringsRoot");
            stringsRoot.transform.SetParent(fretboardRoot.transform, false);
            stringsRoot.AddComponent<StringRenderer>();

            // ── 4. CameraRig ─────────────────────────────────────────────────────
            GameObject cameraRigGO = new GameObject("CameraRig");
            UnityEngine.Camera cam = cameraRigGO.AddComponent<UnityEngine.Camera>();
            cam.tag = "MainCamera";
            cam.fieldOfView  = 55f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane  = 100f;

            CameraRig cameraRig = cameraRigGO.AddComponent<CameraRig>();
            cameraRigGO.transform.position = new Vector3(0f, 1.8f, 2.4f);
            cameraRigGO.transform.rotation = Quaternion.Euler(-22f, 0f, 0f);

            // Apply serialized values
            SerializedObject soCam = new SerializedObject(cameraRig);
            soCam.FindProperty("cameraPosition").vector3Value      = new Vector3(0f, 1.8f, 2.4f);
            soCam.FindProperty("cameraRotationEuler").vector3Value = new Vector3(-22f, 0f, 0f);
            soCam.FindProperty("fieldOfView").floatValue           = 55f;
            soCam.FindProperty("nearClipPlane").floatValue         = 0.1f;
            soCam.FindProperty("farClipPlane").floatValue          = 100f;
            soCam.ApplyModifiedProperties();

            // ── 5. Key Light ─────────────────────────────────────────────────────
            GameObject keyLightGO = new GameObject("KeyLight");
            Light keyLight = keyLightGO.AddComponent<Light>();
            keyLight.type      = LightType.Directional;
            keyLight.intensity = 1.2f;
            keyLight.color     = Color.white;
            keyLightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // ── 6. Fill Light ─────────────────────────────────────────────────────
            GameObject fillLightGO = new GameObject("FillLight");
            Light fillLight = fillLightGO.AddComponent<Light>();
            fillLight.type      = LightType.Directional;
            fillLight.intensity = 0.4f;
            // Warm fill color: #FFF0D0
            fillLight.color = new Color(1f, 0.941f, 0.816f, 1f);
            fillLightGO.transform.rotation = Quaternion.Euler(-20f, 120f, 0f);

            // ── 7. Background Quad ───────────────────────────────────────────────
            GameObject backgroundGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundGO.name = "Background";
            backgroundGO.transform.position = new Vector3(0f, 0f, 14f);
            backgroundGO.transform.rotation = Quaternion.identity; // face camera (Z=0,0,0)
            backgroundGO.transform.localScale = new Vector3(24f, 14f, 1f);

            // Assign a dark material to the background
            Material bgMat = CreateBackgroundMaterial();
            backgroundGO.GetComponent<MeshRenderer>().sharedMaterial = bgMat;

            // Remove the default collider on the background quad — not needed
            Collider bgCollider = backgroundGO.GetComponent<Collider>();
            if (bgCollider != null)
                Object.DestroyImmediate(bgCollider);

            // ── 8. Ensure Scenes folder exists ───────────────────────────────────
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            // ── 9. Save scene ────────────────────────────────────────────────────
            string scenePath = "Assets/Scenes/Milestone1.unity";
            bool saved = EditorSceneManager.SaveScene(newScene, scenePath);

            if (saved)
            {
                AssetDatabase.Refresh();
                Debug.Log("✅ Milestone 1 scene created. Press Play to verify.");
                Debug.Log($"   Scene saved to: {scenePath}");
                Debug.Log("   Objects in scene:\n" +
                          "     • FretboardRoot (FretboardMesh)\n" +
                          "       └── StringsRoot (StringRenderer)\n" +
                          "     • CameraRig (Camera, CameraRig)\n" +
                          "     • KeyLight (Directional, intensity 1.2)\n" +
                          "     • FillLight (Directional, warm, intensity 0.4)\n" +
                          "     • Background (Quad, dark material)");
            }
            else
            {
                Debug.LogError("[MilestoneOneSetup] Failed to save scene to " + scenePath);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Menu: GuitarExercise/Verify/Log Camera Info
        // ─────────────────────────────────────────────────────────────────────────

        [MenuItem("GuitarExercise/Verify/Log Camera Info")]
        public static void LogCameraInfo()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[MilestoneOneSetup] No main camera found in the current scene.");
                return;
            }

            float fov       = cam.fieldOfView;
            Vector3 pos     = cam.transform.position;
            Vector3 rot     = cam.transform.rotation.eulerAngles;
            float aspect    = cam.aspect;

            // Distance from camera to fretboard (y=0 plane)
            float pitchDeg  = rot.x > 180f ? rot.x - 360f : rot.x; // normalize to -180..180
            float pitchRad  = Mathf.Abs(pitchDeg) * Mathf.Deg2Rad;
            float distToBoard = (pitchRad > 0.001f) ? (pos.y / Mathf.Sin(pitchRad)) : pos.y;

            float halfFovRad   = (fov * 0.5f) * Mathf.Deg2Rad;
            float visibleHeight = 2f * distToBoard * Mathf.Tan(halfFovRad);
            float visibleWidth  = visibleHeight * aspect;

            Debug.Log($"[GuitarExercise Camera Info] ─────────────────────────────\n" +
                      $"  Camera Name    : {cam.name}\n" +
                      $"  Tag            : {cam.tag}\n" +
                      $"  Position       : {pos}\n" +
                      $"  Rotation       : {rot}  (pitch effective: {pitchDeg:F1}°)\n" +
                      $"  FOV            : {fov}°\n" +
                      $"  Near/Far Clip  : {cam.nearClipPlane} / {cam.farClipPlane}\n" +
                      $"  Aspect Ratio   : {aspect:F3}\n" +
                      $"  Dist to Board  : {distToBoard:F3} units\n" +
                      $"  Visible Width  : {visibleWidth:F3} units  (need ≥ 0.45 for full board)\n" +
                      $"  Visible Height : {visibleHeight:F3} units\n" +
                      $"  ─────────────────────────────────────────────────────────");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────────

        private static Material CreateBackgroundMaterial()
        {
            // Use Universal Render Pipeline/Unlit — always available in URP projects
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Standard"); // absolute fallback

            Material mat = new Material(shader);
            // Dark near-black background: #0C0C0E
            mat.color = new Color(0.047f, 0.047f, 0.055f, 1f);
            mat.name = "BackgroundMaterial";

            // Save the material as an asset so the scene can reference it
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");

            string matPath = "Assets/Materials/BackgroundMaterial.mat";
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }

        // Validate menu items — only available when not in play mode
        [MenuItem("GuitarExercise/Setup/Create Milestone 1 Scene", true)]
        private static bool ValidateCreateScene()
        {
            return !EditorApplication.isPlaying;
        }

        [MenuItem("GuitarExercise/Verify/Log Camera Info", true)]
        private static bool ValidateLogCameraInfo()
        {
            return true; // available always
        }
    }
}
#endif
