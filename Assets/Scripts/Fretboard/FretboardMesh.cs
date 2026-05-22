using System.Collections.Generic;
using UnityEngine;

namespace GuitarExerciseView.Fretboard
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FretboardMesh : MonoBehaviour
    {
        [Header("Fretboard Dimensions")]
        public int numFrets = 22;
        public float scaleLength = 12f;
        public float nutWidth = 0.38f;
        public float bodyWidth = 0.45f;

        [Header("Visual Settings")]
        public bool showGizmos = true;
        // Option C: fret lines removed — string lanes + note pills carry all gameplay info. Held notes and bends travel full lane length without fret snapping.
        public bool showFretLines = false;

        [Header("Fret Wire Settings")]
        [SerializeField] private float fretWireHeight = 0.008f;
        [SerializeField] private float fretWireWidth = 0.004f;

        [Header("Fretboard Radius")]
        [SerializeField] private float fretboardRadiusUnits = 0.406f; // 16" radius scaled

        [Header("Materials")]
        [SerializeField] private Material fretboardMaterial;
        [SerializeField] private Material fretWireMaterial;
        [SerializeField] private Material inlayMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        // Child GameObjects for fret wires and inlays
        private GameObject _fretWiresRoot;
        private GameObject _inlaysRoot;

        // Inlay fret positions: single dots, then double at 12
        private static readonly int[] SingleInlayFrets = { 3, 5, 7, 9, 15, 17, 19, 21 };
        private static readonly int[] DoubleInlayFrets = { 12 };

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            RegenerateMesh();
        }

        /// <summary>
        /// Calculates the position of fret n along the scale length using equal temperament formula.
        /// pos[0] = 0 (nut), pos[n] = scaleLength * (1 - 1/2^(n/12))
        /// </summary>
        private float GetFretPosition(int n)
        {
            if (n == 0) return 0f;
            return scaleLength * (1f - 1f / Mathf.Pow(2f, n / 12f));
        }

        /// <summary>
        /// Returns the fretboard width at a given position along the scale length (0..scaleLength).
        /// Linear taper from nutWidth at 0 to bodyWidth at scaleLength.
        /// </summary>
        private float GetWidthAtPosition(float pos)
        {
            float t = (scaleLength > 0f) ? (pos / scaleLength) : 0f;
            return Mathf.Lerp(nutWidth, bodyWidth, t);
        }

        /// <summary>
        /// Applies the fretboard radius curvature to a vertex. The board curves upward at the edges.
        /// radius in scene units. The Y offset = radius - sqrt(radius^2 - x^2) subtracted from y
        /// so the center is highest and the edges curve down (or the center is flat and edges dip).
        /// We use the convention: center (x=0) is the reference height; edges dip slightly.
        /// </summary>
        private float GetRadiusYOffset(float x)
        {
            float r = fretboardRadiusUnits;
            // Clamp x to avoid sqrt of negative
            float xClamped = Mathf.Clamp(x, -r + 0.001f, r - 0.001f);
            return r - Mathf.Sqrt(r * r - xClamped * xClamped);
        }

        public void RegenerateMesh()
        {
            // Clean up existing children
            DestroyChildrenOf(ref _fretWiresRoot, "FretWires");
            DestroyChildrenOf(ref _inlaysRoot, "Inlays");

            BuildFretboardMesh();
            // BuildFretWires(); // Disabled — showFretLines = false (Option C). Re-enable here and below if fret wire geometry is ever needed.
            BuildInlays();

            if (fretboardMaterial != null)
                _meshRenderer.sharedMaterial = fretboardMaterial;
        }

        private void DestroyChildrenOf(ref GameObject root, string name)
        {
            if (root != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(root);
#else
                Destroy(root);
#endif
            }
            root = new GameObject(name);
            root.transform.SetParent(transform, false);
        }

        private void BuildFretboardMesh()
        {
            // Fretboard surface only — no fret wire geometry. See docs/sources.md for design decision.
            // We'll create a subdivided quad mesh along the fretboard length.
            // For each segment between fret positions we generate a quad.
            // Number of longitudinal segments = numFrets + 1 (from nut to last fret + body overhang)
            // We also subdivide across width into widthSegments for the radius curve.
            int widthSegments = 8; // subdivisions across the width for radius curvature
            int lengthSegments = numFrets + 1; // one segment past last fret

            // Compute fret positions
            float[] fretPos = new float[numFrets + 1];
            for (int i = 0; i <= numFrets; i++)
                fretPos[i] = GetFretPosition(i);

            // Add a body-end segment past the last fret
            float[] segmentPositions = new float[lengthSegments + 1];
            for (int i = 0; i <= numFrets; i++)
                segmentPositions[i] = fretPos[i];
            // Body end extends 5% past scale length
            segmentPositions[numFrets + 1] = scaleLength * 1.05f;

            int vertCountX = widthSegments + 1;
            int vertCountZ = lengthSegments + 1;
            int totalVerts = vertCountX * vertCountZ;

            Vector3[] vertices = new Vector3[totalVerts];
            Vector2[] uvs = new Vector2[totalVerts];
            Vector3[] normals = new Vector3[totalVerts];

            for (int zi = 0; zi < vertCountZ; zi++)
            {
                float zPos = segmentPositions[zi];
                float width = GetWidthAtPosition(zPos);
                float halfWidth = width * 0.5f;
                float uZ = zPos / (scaleLength * 1.05f);

                for (int xi = 0; xi < vertCountX; xi++)
                {
                    float tX = (float)xi / widthSegments;
                    float xLocal = Mathf.Lerp(-halfWidth, halfWidth, tX);
                    float yOffset = -GetRadiusYOffset(xLocal * (0.406f / halfWidth)); // normalize x to radius space
                    // Actually compute proper radius offset
                    float normalizedX = (halfWidth > 0f) ? (xLocal / halfWidth) * fretboardRadiusUnits : 0f;
                    float radiusY = -GetRadiusYOffset(normalizedX);

                    int idx = zi * vertCountX + xi;
                    vertices[idx] = new Vector3(xLocal, radiusY, zPos);
                    uvs[idx] = new Vector2(uZ, tX);
                    normals[idx] = Vector3.up; // approximate; could compute per-vertex later
                }
            }

            // Build triangles
            int triCount = widthSegments * lengthSegments * 2;
            int[] triangles = new int[triCount * 3];
            int t = 0;
            for (int zi = 0; zi < lengthSegments; zi++)
            {
                for (int xi = 0; xi < widthSegments; xi++)
                {
                    int bl = zi * vertCountX + xi;
                    int br = bl + 1;
                    int tl = bl + vertCountX;
                    int tr = tl + 1;

                    triangles[t++] = bl;
                    triangles[t++] = tl;
                    triangles[t++] = tr;

                    triangles[t++] = bl;
                    triangles[t++] = tr;
                    triangles[t++] = br;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "FretboardMesh";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _meshFilter.sharedMesh = mesh;
        }

        private void BuildFretWires()
        {
            // ── FRET WIRE GEOMETRY REMOVED (Option C) ────────────────────────────────
            // Fret wire quads (thin raised cross-board meshes) are intentionally omitted.
            // String lane GameObjects and note-pill renderers carry all positional info at
            // runtime.  Held notes and bend visuals travel the full lane length without
            // snapping to fret boundaries.  See docs/sources.md for the full design note.
            //
            // To restore: uncomment the loop body below and re-enable the BuildFretWires()
            // call in RegenerateMesh().  fretWireHeight, fretWireWidth, and fretWireMaterial
            // serialised fields are retained so existing scene references are not broken.
            // ─────────────────────────────────────────────────────────────────────────
        }

        private Material CreateDefaultFretWireMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.8f, 0.8f, 0.75f, 1f);
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Smoothness", 0.8f);
            return mat;
        }

        private void BuildInlays()
        {
            // Single inlays
            foreach (int fretNum in SingleInlayFrets)
            {
                if (fretNum > numFrets) continue;
                float z1 = GetFretPosition(fretNum - 1);
                float z2 = GetFretPosition(fretNum);
                float zCenter = (z1 + z2) * 0.5f;
                CreateInlayDot($"Inlay_{fretNum}", zCenter, 0f);
            }

            // Double inlays (two dots side by side)
            foreach (int fretNum in DoubleInlayFrets)
            {
                if (fretNum > numFrets) continue;
                float z1 = GetFretPosition(fretNum - 1);
                float z2 = GetFretPosition(fretNum);
                float zCenter = (z1 + z2) * 0.5f;
                float width = GetWidthAtPosition(zCenter);
                float spacing = width * 0.25f;
                CreateInlayDot($"Inlay_{fretNum}_L", zCenter, -spacing);
                CreateInlayDot($"Inlay_{fretNum}_R", zCenter, spacing);
            }
        }

        private void CreateInlayDot(string name, float zPos, float xOffset)
        {
            GameObject inlayGO = new GameObject(name);
            inlayGO.transform.SetParent(_inlaysRoot.transform, false);

            MeshFilter mf = inlayGO.AddComponent<MeshFilter>();
            MeshRenderer mr = inlayGO.AddComponent<MeshRenderer>();

            if (inlayMaterial != null)
                mr.sharedMaterial = inlayMaterial;
            else
                mr.sharedMaterial = CreateDefaultInlayMaterial();

            // Create a circle polygon (12 sides) for the inlay dot
            int segments = 12;
            float radius = 0.018f;
            float yPos = 0.001f; // slightly above fretboard surface

            Vector3[] verts = new Vector3[segments + 1];
            Vector2[] uvs = new Vector2[segments + 1];
            int[] tris = new int[segments * 3];

            verts[0] = new Vector3(xOffset, yPos, zPos);
            uvs[0] = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = xOffset + Mathf.Cos(angle) * radius;
                float z = zPos + Mathf.Sin(angle) * radius;
                verts[i + 1] = new Vector3(x, yPos, z);
                uvs[i + 1] = new Vector2(Mathf.Cos(angle) * 0.5f + 0.5f, Mathf.Sin(angle) * 0.5f + 0.5f);
            }

            for (int i = 0; i < segments; i++)
            {
                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = (i + 1) % segments + 1;
            }

            Mesh dotMesh = new Mesh();
            dotMesh.name = $"{name}_Mesh";
            dotMesh.vertices = verts;
            dotMesh.uv = uvs;
            dotMesh.triangles = tris;
            dotMesh.RecalculateNormals();
            dotMesh.RecalculateBounds();
            mf.sharedMesh = dotMesh;
        }

        private Material CreateDefaultInlayMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Standard"));
            // Pearl-white inlay
            mat.color = new Color(0.9f, 0.88f, 0.85f, 1f);
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.7f);
            return mat;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = new Color(0.4f, 0.8f, 0.4f, 0.4f);
            // Draw wire cube approximating fretboard bounds
            float centerZ = scaleLength * 0.5f;
            float avgWidth = (nutWidth + bodyWidth) * 0.5f;
            Vector3 center = transform.TransformPoint(new Vector3(0f, 0f, centerZ));
            Vector3 size = new Vector3(avgWidth, 0.05f, scaleLength);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 0f, centerZ), size);
            Gizmos.matrix = Matrix4x4.identity;

            // Draw fret positions as lines
            Gizmos.color = new Color(0.8f, 0.8f, 0.3f, 0.7f);
            for (int i = 0; i <= numFrets; i++)
            {
                float zPos = GetFretPosition(i);
                float width = GetWidthAtPosition(zPos);
                Vector3 left = transform.TransformPoint(new Vector3(-width * 0.5f, 0.01f, zPos));
                Vector3 right = transform.TransformPoint(new Vector3(width * 0.5f, 0.01f, zPos));
                Gizmos.DrawLine(left, right);
            }
        }
#endif
    }
}
