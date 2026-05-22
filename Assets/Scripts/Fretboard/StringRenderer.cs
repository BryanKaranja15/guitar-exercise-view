using System.Collections;
using UnityEngine;

namespace GuitarExerciseView.Fretboard
{
    /// <summary>
    /// Placeholder for the guitar skin data system.
    /// Used by StringRenderer.UpdateStrings() for future skin swapping.
    /// </summary>
    [System.Serializable]
    public class GuitarSkinData
    {
        public string skinName = "Default";
        public Color[] stringColors = new Color[6];
        public float[] stringGauges = new float[6];
    }

    /// <summary>
    /// Renders the 6 guitar strings as LineRenderer components on child GameObjects.
    /// Supports animated string vibration via AnimateStringHit().
    /// </summary>
    public class StringRenderer : MonoBehaviour
    {
        [System.Serializable]
        public struct StringData
        {
            public int stringIndex;       // 0=high e, 5=low E
            public float gaugeInches;     // e.g. 0.010f
            public Color stringColor;
            public float yPosition;       // y offset above fretboard surface
            public float zOffset;         // for layering / depth offset
        }

        [Header("String Settings")]
        [SerializeField] private float scaleLength = 12f;
        [SerializeField] private float widthMultiplierBase = 2.8f;
        [SerializeField] private float stringHeightAboveBoard = 0.012f;

        [Header("String Data")]
        [SerializeField] private StringData[] strings = new StringData[6];

        // LineRenderer references — one per string
        private LineRenderer[] _lineRenderers = new LineRenderer[6];
        private GameObject[] _stringObjects = new GameObject[6];

        // Coroutine handles for vibration animation
        private Coroutine[] _vibrationCoroutines = new Coroutine[6];

        // Default gauges (standard light set: .010-.046)
        private static readonly float[] DefaultGauges =
            { 0.010f, 0.013f, 0.017f, 0.026f, 0.036f, 0.046f };

        // high e (0) to low E (5)
        // Plain strings: 0,1,2 => metallic silver
        // Wound strings: 3,4,5 => warm tinted
        private static readonly Color SilverColor = new Color(0.753f, 0.753f, 0.753f, 1f);   // #C0C0C0
        private static readonly Color WoundColor  = new Color(0.722f, 0.659f, 0.471f, 1f);   // #B8A878

        private void Awake()
        {
            InitializeDefaultStringData();
            CreateStringObjects();
        }

        private void InitializeDefaultStringData()
        {
            for (int i = 0; i < 6; i++)
            {
                strings[i] = new StringData
                {
                    stringIndex = i,
                    gaugeInches = DefaultGauges[i],
                    stringColor = (i < 3) ? SilverColor : WoundColor,
                    yPosition = stringHeightAboveBoard,
                    zOffset = i * 0.0001f  // tiny z separation to prevent z-fighting between strings
                };
            }
        }

        private void CreateStringObjects()
        {
            // Remove existing child string objects if any
            foreach (var go in _stringObjects)
            {
                if (go != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(go);
#else
                    Destroy(go);
#endif
                }
            }

            for (int i = 0; i < 6; i++)
            {
                GameObject stringGO = new GameObject($"String_{i:D2}_{GetStringName(i)}");
                stringGO.transform.SetParent(transform, false);
                _stringObjects[i] = stringGO;

                LineRenderer lr = stringGO.AddComponent<LineRenderer>();
                _lineRenderers[i] = lr;

                ConfigureLineRenderer(lr, i);
            }
        }

        private void ConfigureLineRenderer(LineRenderer lr, int stringIdx)
        {
            StringData data = strings[stringIdx];

            // Width proportional to gauge
            float lineWidth = data.gaugeInches * widthMultiplierBase;

            lr.useWorldSpace = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.positionCount = 2;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.startColor = data.stringColor;
            lr.endColor = data.stringColor;
            lr.numCapVertices = 4;
            lr.numCornerVertices = 4;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lr.generateLightingData = true;

            // Assign a default material if none set
            Material mat = CreateStringMaterial(data.stringColor);
            lr.sharedMaterial = mat;

            // Calculate evenly spaced X positions across fretboard width.
            // String 0 = high e (thinnest) on one edge, string 5 = low E on the other.
            // Using 0.38 as nut width reference — spaced evenly across the nut.
            float nutWidth = 0.38f;
            float stringSpacing = nutWidth / 5f; // 5 gaps for 6 strings
            float xPos = (-nutWidth * 0.5f) + (stringIdx * stringSpacing);

            float yPos = data.yPosition;
            float zOff = data.zOffset;

            lr.SetPosition(0, new Vector3(xPos, yPos, 0f));
            lr.SetPosition(1, new Vector3(xPos, yPos, scaleLength));
        }

        private Material CreateStringMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = color;
            return mat;
        }

        private string GetStringName(int index)
        {
            // Standard tuning: e B G D A E (high to low)
            string[] names = { "e", "B", "G", "D", "A", "E" };
            return (index >= 0 && index < names.Length) ? names[index] : index.ToString();
        }

        /// <summary>
        /// Updates all string appearances from a GuitarSkinData asset.
        /// Intended for future skin system hookup.
        /// </summary>
        public void UpdateStrings(GuitarSkinData skin)
        {
            if (skin == null)
            {
                Debug.LogWarning("[StringRenderer] UpdateStrings called with null skin.");
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                if (i < skin.stringColors.Length)
                    strings[i].stringColor = skin.stringColors[i];
                if (i < skin.stringGauges.Length)
                    strings[i].gaugeInches = skin.stringGauges[i];

                if (_lineRenderers[i] != null)
                {
                    _lineRenderers[i].startColor = strings[i].stringColor;
                    _lineRenderers[i].endColor   = strings[i].stringColor;

                    float lineWidth = strings[i].gaugeInches * widthMultiplierBase;
                    _lineRenderers[i].startWidth = lineWidth;
                    _lineRenderers[i].endWidth   = lineWidth;

                    Material mat = CreateStringMaterial(strings[i].stringColor);
                    _lineRenderers[i].sharedMaterial = mat;
                }
            }
        }

        /// <summary>
        /// Triggers a pluck vibration animation on the specified string (0..5).
        /// The string oscillates in a sine-wave decay over 0.4 seconds.
        /// </summary>
        public void AnimateStringHit(int stringIndex)
        {
            if (stringIndex < 0 || stringIndex >= 6) return;
            if (_lineRenderers[stringIndex] == null) return;

            // Cancel any in-progress vibration on this string
            if (_vibrationCoroutines[stringIndex] != null)
                StopCoroutine(_vibrationCoroutines[stringIndex]);

            _vibrationCoroutines[stringIndex] = StartCoroutine(VibrationCoroutine(stringIndex));
        }

        private IEnumerator VibrationCoroutine(int stringIndex)
        {
            LineRenderer lr = _lineRenderers[stringIndex];
            if (lr == null) yield break;

            float duration = 0.4f;
            float elapsed = 0f;
            float amplitude = 0.015f;         // max displacement in world units
            float frequency = 18f;             // oscillations per second
            int midPoints = 8;                 // number of intermediate points for the wave shape

            // Compute base start/end positions
            StringData data = strings[stringIndex];
            float nutWidth = 0.38f;
            float stringSpacing = nutWidth / 5f;
            float xPos = (-nutWidth * 0.5f) + (stringIndex * stringSpacing);
            float yBase = data.yPosition;

            // Switch to multi-point mode
            lr.positionCount = midPoints + 2;

            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float decay = 1f - progress;                          // linear decay
                float currentAmplitude = amplitude * decay * decay;   // quadratic decay feels natural
                float phase = elapsed * frequency * Mathf.PI * 2f;

                // Set start point
                lr.SetPosition(0, new Vector3(xPos, yBase, 0f));

                // Set intermediate points with sine wave
                for (int p = 0; p < midPoints; p++)
                {
                    float t = (float)(p + 1) / (midPoints + 1);
                    float zPos = t * scaleLength;
                    // Envelope: peaks at t=0.5, zero at both ends
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    float displacement = currentAmplitude * envelope * Mathf.Sin(phase + t * Mathf.PI * 2f);
                    lr.SetPosition(p + 1, new Vector3(xPos, yBase + displacement, zPos));
                }

                // Set end point
                lr.SetPosition(midPoints + 1, new Vector3(xPos, yBase, scaleLength));

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Restore to 2-point straight line
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(xPos, yBase, 0f));
            lr.SetPosition(1, new Vector3(xPos, yBase, scaleLength));

            _vibrationCoroutines[stringIndex] = null;
        }

        private void OnValidate()
        {
            // Allow live updates in editor when values change
            if (_lineRenderers != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (_lineRenderers[i] != null)
                        ConfigureLineRenderer(_lineRenderers[i], i);
                }
            }
        }
    }
}
