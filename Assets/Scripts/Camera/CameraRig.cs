using UnityEngine;

namespace GuitarExerciseView.Camera
{
    /// <summary>
    /// Positions and configures the main camera for the guitar exercise view.
    /// Default position: (0, 1.8, 2.4) looking down at the fretboard at -22° pitch.
    /// FOV: 55° — wide enough to see full fretboard width at comfortable distance.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [Header("Camera Transform")]
        [SerializeField] private Vector3 cameraPosition = new Vector3(0f, 1.8f, 2.4f);
        [SerializeField] private Vector3 cameraRotationEuler = new Vector3(-22f, 0f, 0f);

        [Header("Camera Settings")]
        [SerializeField] private float fieldOfView = 55f;
        [SerializeField] private float nearClipPlane = 0.1f;
        [SerializeField] private float farClipPlane = 100f;

        [Header("Editor Preview")]
        [SerializeField] private bool livePreview = false;

        private void Awake()
        {
            ApplyCameraSettings();
        }

        [ContextMenu("Apply Camera Settings")]
        public void ApplyCameraSettings()
        {
            UnityEngine.Camera cam = GetTargetCamera();
            if (cam == null)
            {
                Debug.LogWarning("[CameraRig] No camera found. Attach this component to a Camera GameObject or ensure Camera.main exists.");
                return;
            }

            cam.fieldOfView  = fieldOfView;
            cam.nearClipPlane = nearClipPlane;
            cam.farClipPlane  = farClipPlane;

            cam.transform.position = cameraPosition;
            cam.transform.rotation = Quaternion.Euler(cameraRotationEuler);

            LogVisibleWorldUnits(cam);
        }

        private UnityEngine.Camera GetTargetCamera()
        {
            // First try the Camera component on this object or its children
            UnityEngine.Camera cam = GetComponent<UnityEngine.Camera>();
            if (cam != null) return cam;

            cam = GetComponentInChildren<UnityEngine.Camera>();
            if (cam != null) return cam;

            // Fall back to Camera.main
            return UnityEngine.Camera.main;
        }

        /// <summary>
        /// Calculates the visible world units at the fretboard plane distance and logs them.
        /// This helps developers verify the entire fretboard width is visible.
        /// </summary>
        private void LogVisibleWorldUnits(UnityEngine.Camera cam)
        {
            // The fretboard lies at y=0, camera looks down at -22°.
            // Calculate the distance from camera to the fretboard plane (y=0).
            // Camera position is at y=1.8, pitch=-22°, so the view ray hits y=0 at some distance.
            float camY = cam.transform.position.y;
            float pitchRad = Mathf.Abs(cameraRotationEuler.x) * Mathf.Deg2Rad;
            float distanceToBoard = (pitchRad > 0.001f) ? (camY / Mathf.Sin(pitchRad)) : camY;

            // Visible height at that distance
            float halfFovRad = (cam.fieldOfView * 0.5f) * Mathf.Deg2Rad;
            float visibleHeight = 2f * distanceToBoard * Mathf.Tan(halfFovRad);

            // Visible width (account for aspect ratio)
            float aspect = cam.aspect;
            float visibleWidth = visibleHeight * aspect;

            Debug.Log($"[CameraRig] ─── Camera Info ───────────────────────────────\n" +
                      $"  Position       : {cam.transform.position}\n" +
                      $"  Rotation       : {cam.transform.rotation.eulerAngles}\n" +
                      $"  FOV            : {cam.fieldOfView}°\n" +
                      $"  Aspect Ratio   : {aspect:F3}\n" +
                      $"  Dist to Board  : {distanceToBoard:F3} units\n" +
                      $"  Visible Width  : {visibleWidth:F3} units  (fretboard body width: 0.45)\n" +
                      $"  Visible Height : {visibleHeight:F3} units\n" +
                      $"  ──────────────────────────────────────────────────");
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (livePreview)
            {
                ApplyCameraSettings();
            }
        }

        private void OnDrawGizmosSelected()
        {
            UnityEngine.Camera cam = GetTargetCamera();
            if (cam == null) return;

            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.8f);
            Gizmos.matrix = Matrix4x4.TRS(cam.transform.position, cam.transform.rotation, Vector3.one);

            // Draw FOV frustum lines
            float halfFovRad = (fieldOfView * 0.5f) * Mathf.Deg2Rad;
            float dist = 5f;
            float halfH = dist * Mathf.Tan(halfFovRad);
            float halfW = halfH * 1.77f; // approx 16:9

            Vector3 tl = new Vector3(-halfW,  halfH, dist);
            Vector3 tr = new Vector3( halfW,  halfH, dist);
            Vector3 bl = new Vector3(-halfW, -halfH, dist);
            Vector3 br = new Vector3( halfW, -halfH, dist);

            Gizmos.DrawLine(Vector3.zero, tl);
            Gizmos.DrawLine(Vector3.zero, tr);
            Gizmos.DrawLine(Vector3.zero, bl);
            Gizmos.DrawLine(Vector3.zero, br);
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            Gizmos.DrawLine(bl, tl);
        }
#endif
    }
}
