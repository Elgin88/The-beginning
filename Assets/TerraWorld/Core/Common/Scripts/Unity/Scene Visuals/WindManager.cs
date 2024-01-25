using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime
{
    public struct WindManagerParams
    {
        public bool hasWind;
        public float windMain;
        public float windTurbulence;
        public float windPulseMagnitude;
        public float windPulseFrequency;
        public float windDirection;
        public bool randomWind;
        public float randomWindTime;
        public bool showWindGizmo;
    }

    [ExecuteAlways]
    [RequireComponent(typeof(WindZone))]
    public class WindManager : MonoBehaviour
    {
        public static bool Enabled { get => Script.enabled; set => Script.enabled = value; }
        private static WindManager Script { get => GetScript(); }
        public static float WindDirection { get => Script.windDirection; }
        private static WindManager _script;

        private static WindManager GetScript()
        {
            if (_script == null)
            {
                WindManager script = TTerraWorldManager.SceneSettingsGO1.GetComponent<WindManager>();

                if (script != null)
                    _script = script;
                else
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<WindManager>();
            }

            return _script;
        }

        public bool hasWind = true;
        [Range(0f, 360f)] public float windDirection;
        public bool randomWind = true;
        public float randomWindTime = 1f;
        public bool showWindGizmo = false;
        //public float windSpeedNormalized;
        public AudioClip windSound;

        private float randomWindSpeed;
        private int _TerrainLODWindID = Shader.PropertyToID("_TerrainLODWind");
        private int _WindSpeedMultiplierID = Shader.PropertyToID("_WindSpeedMultiplier");
        private int _WindSpeedMultiplierGrassID = Shader.PropertyToID("_WindSpeedMultiplierGrass");
        private Color gizmoColor = new Color(0.2f, 0.2f, 1f, 1f);
        private GUIStyle guiStyle;
        private const float windSpeedMin = 0.01f;
        private const float windSpeedMax = 5f;
        private AudioSource audioSource;

        public static Vector3 windDirectionForward
        {
            get
            {
                float WindStrength = WindZone.windMain + WindZone.windPulseMagnitude * (1.0f + Mathf.Sin(Time.time * WindZone.windPulseFrequency) + 1.0f + Mathf.Sin(Time.time * WindZone.windPulseFrequency * 3.0f)) * 0.5f;
                Vector3 _windDirectionForward = Quaternion.Euler(new Vector3(0, Script.windDirection, 0)) * Vector3.back;
                _windDirectionForward.x *= WindStrength;
                _windDirectionForward.y *= WindStrength;
                _windDirectionForward.z *= WindStrength;
                return _windDirectionForward;
            }
        }

        private static WindManagerParams _parameters;

        public static WindManagerParams GetParams()
        {
            _parameters.hasWind = Script.hasWind;
            _parameters.windMain = WindZone.windMain;
            _parameters.windTurbulence = WindZone.windTurbulence;
            _parameters.windPulseMagnitude = WindZone.windPulseMagnitude;
            _parameters.windPulseFrequency = WindZone.windPulseFrequency;
            _parameters.windDirection = Script.windDirection;
            _parameters.randomWind = Script.randomWind;
            _parameters.randomWindTime = Script.randomWindTime;
            _parameters.showWindGizmo = Script.showWindGizmo;

            return _parameters;
        }

        public static void SetParams(WindManagerParams parameters)
        {
            _parameters = parameters;

            Script.hasWind = parameters.hasWind;
            WindZone.windMain = parameters.windMain;
            WindZone.windTurbulence = parameters.windTurbulence;
            WindZone.windPulseMagnitude = parameters.windPulseMagnitude;
            WindZone.windPulseFrequency = parameters.windPulseFrequency;
            Script.windDirection = parameters.windDirection;
            Script.randomWind = parameters.randomWind;
            Script.randomWindTime = parameters.randomWindTime;
            Script.showWindGizmo = parameters.showWindGizmo;

            Script.Apply();
        }

        private static WindZone _windZone;
        public static WindZone WindZone
        {
            get
            {
                if (_windZone == null)
                {
                    _windZone = TTerraWorldManager.SceneSettingsGO1.GetComponent<WindZone>();

                    if (_windZone == null)
                        _windZone = TTerraWorldManager.SceneSettingsGO1.AddComponent<WindZone>();
                }

                return _windZone;
            }
        }

        public void OnValidate()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;
            Apply();
        }

        private void OnEnable()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;
            Apply();
        }

        void Update()
        {
            if (WindZone != null)
            {
                if (randomWind)
                {
                    float randomTime = randomWindTime / 10f;

                    randomWindSpeed = Mathf.Clamp
                    (
                        WindZone.windMain * (1.0f + Mathf.Sin(Time.time * randomTime) + 1.0f + Mathf.Sin(Time.time * randomTime * 3.0f)) * 0.5f,
                        Mathf.Clamp(WindZone.windMain - 0.666f, windSpeedMin, windSpeedMax),
                        Mathf.Clamp(WindZone.windMain + 2f, windSpeedMin, windSpeedMax)
                    );
                }
                else
                    randomWindSpeed = WindZone.windMain;

                WindZone.windMain = Mathf.Clamp(WindZone.windMain, windSpeedMin, windSpeedMax);
                //WindZone.windMain = Mathf.Clamp(WindZone.windMain * Mathf.Exp(TimeOfDay.GetParams().dayNightCycleSpeed), windSpeedMin, windSpeedMax);

                WindZone.windTurbulence = Mathf.Clamp(WindZone.windTurbulence, 0.001f, 3);
                WindZone.windPulseMagnitude = Mathf.Clamp(WindZone.windPulseMagnitude, 0.001f, 5);
                WindZone.windPulseFrequency = Mathf.Clamp(WindZone.windPulseFrequency, 0.001f, 2);
                UpdateWindSettingsGLOBAL();

                if (audioSource != null)
                    audioSource.pitch = randomWindSpeed;

                //windSpeedNormalized = Mathf.InverseLerp(windSpeedMin, windSpeedMax, randomWindSpeed);
            }

            // Shows grass touch bending in the editor
            if (!Application.isPlaying)
                GrassBendingManager.ProcessBenders();
        }

        public void Apply()
        {
            guiStyle = new GUIStyle();
            guiStyle.alignment = TextAnchor.MiddleCenter;
            guiStyle.fontSize = 14;
            guiStyle.normal.textColor = gizmoColor;
            guiStyle.fontStyle = FontStyle.Bold;

            if (windSound != null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = windSound;
                audioSource.playOnAwake = true;
                audioSource.loop = true;
                audioSource.spatialBlend = 0f;
                audioSource.Play();
            }
            else
            {
                audioSource = GetComponent<AudioSource>();

                if (audioSource != null)
                {
                    if (Application.isPlaying) Destroy(audioSource);
                    else DestroyImmediate(audioSource);
                }
            }

            UpdateWindSettingsGLOBAL();
        }

        private void UpdateWindSettingsGLOBAL()
        {
            if (hasWind)
            {
                Shader.SetGlobalFloat("_WindState", 1);
                Shader.SetGlobalVector(_TerrainLODWindID, new Vector4(windDirectionForward.x, windDirectionForward.y, windDirectionForward.z, WindZone.windTurbulence));
                Shader.SetGlobalFloat(_WindSpeedMultiplierID, randomWindSpeed);
                Shader.SetGlobalFloat(_WindSpeedMultiplierGrassID, randomWindSpeed);
            }
            else
                Shader.SetGlobalFloat("_WindState", 0);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        //private void OnDrawGizmosSelected()
        {
            if (!showWindGizmo) return;
            Vector3 gizmoPosition = TCameraManager.CurrentCamera.transform.position + (TCameraManager.CurrentCamera.transform.forward * 15);
            Handles.color = gizmoColor;
            Handles.ArrowHandleCap(0, gizmoPosition, Quaternion.Euler(new Vector3(0, windDirection, 0)), Mathf.Clamp(3f * randomWindSpeed, 3f, 10f), EventType.Repaint);
            Handles.Label(gizmoPosition + (TCameraManager.CurrentCamera.transform.up * 0.33f), "WIND DIRECTION", guiStyle);
        }
#endif
    }
}

