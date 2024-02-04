using UnityEngine;
using System;
using TerraUnity.Runtime;
using TerraUnity.Edittime;

namespace TerraUnity
{
    public class TCameraManager
    {
        public static Camera CurrentCamera { get => GetCurrentCamera(); }
        private static Camera _currentCamera = null;

        public static Camera MainCamera { get => GetMainCamera(); }

#if UNITY_EDITOR
        public static Camera SceneCamera { get => GetSceneCamera(); }
        private static Camera _sceneCamera = null;
#endif

        private static Camera GetCurrentCamera()
        {
            if (_currentCamera != null) return _currentCamera;

#if UNITY_EDITOR
            if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
            {
                Camera temp = UnityEditor.SceneView.lastActiveSceneView.camera;

                if (temp != null && temp.name.Equals("SceneCamera"))
                    _currentCamera = temp;
                //else
                    //throw new Exception("No SceneCamera found in scene!");
            }
            else
            {
                if (_currentCamera == null)
                    _currentCamera = Camera.main;

                if (_currentCamera == null)
                    _currentCamera = Camera.current;

                if (_currentCamera == null)
                {
                    Camera camera = (Camera)MonoBehaviour.FindObjectOfType(typeof(Camera));

                    if (camera)
                        _currentCamera = camera;
                }
            }
#else
            if (_currentCamera == null)
                _currentCamera = Camera.main;

            if (_currentCamera == null)
                _currentCamera = Camera.current;

            if (_currentCamera == null)
            {
                Camera camera = (Camera)MonoBehaviour.FindObjectOfType(typeof(Camera));

                if (camera)
                    _currentCamera = camera;
            }
#endif

            if (_currentCamera == null)
                throw new Exception("No cameras found in scene!");

            return _currentCamera;
        }

        private static Camera GetMainCamera()
        {
            if (Camera.main == null)
                throw new Exception("Camera with tag \"MainCamera\" not found in scene!");
            else
                return Camera.main;
        }

#if UNITY_EDITOR
        private static Camera GetSceneCamera()
        {
            if (_sceneCamera != null) return _sceneCamera;

            if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
            {
                Camera temp = UnityEditor.SceneView.lastActiveSceneView.camera;

                if (temp != null && temp.name.Equals("SceneCamera"))
                    _sceneCamera = temp;
                //else
                    //throw new Exception("No SceneCamera found in scene!");
            }

            return _sceneCamera;
        }
#endif

        public static void GetCameraControls(out bool grabEditorCamMatrix, out bool extendedFlyCam, out bool setTargetFramerate, out int targetFrameRate)
        {
            grabEditorCamMatrix = false;
            extendedFlyCam = false;
            setTargetFramerate = false;
            targetFrameRate = 60;

#if UNITY_EDITOR
            GrabEditorCamMatrix GECM = MainCamera.GetComponent<GrabEditorCamMatrix>();

            if (GECM == null)
                grabEditorCamMatrix = false;
            else
                grabEditorCamMatrix = GECM.enabled;
#endif
            ExtendedFlyCam EFC = MainCamera.GetComponent<ExtendedFlyCam>();

            if (EFC == null)
                extendedFlyCam = false;
            else
                extendedFlyCam = EFC.enabled;

            TTargetFramerate TFR = MainCamera.GetComponent<TTargetFramerate>();

            if (TFR == null)
                setTargetFramerate = false;
            else
                setTargetFramerate = TFR.enabled;

            if (TFR != null) targetFrameRate = TFR.targetFrameRate;
        }

        public static void SetCameraControls(bool grabEditorCamMatrix, bool extendedFlyCam, bool setTargetFramerate, int targetFrameRate)
        {
#if UNITY_EDITOR
            GrabEditorCamMatrix GECM = MainCamera.GetComponent<GrabEditorCamMatrix>();

            if (GECM == null)
            {
                if (grabEditorCamMatrix)
                    GECM = MainCamera.gameObject.AddComponent<GrabEditorCamMatrix>();
            }
            else if (!grabEditorCamMatrix)
                MonoBehaviour.DestroyImmediate(GECM);
#endif
            ExtendedFlyCam EFC = MainCamera.GetComponent<ExtendedFlyCam>();

            if (EFC == null)
            {
                if (extendedFlyCam)
                    EFC = MainCamera.gameObject.AddComponent<ExtendedFlyCam>();
            }
            else if (!extendedFlyCam)
                MonoBehaviour.DestroyImmediate(EFC);

            TTargetFramerate TFR = MainCamera.GetComponent<TTargetFramerate>();

            if (TFR == null)
            {
                if (setTargetFramerate)
                    TFR = MainCamera.gameObject.AddComponent<TTargetFramerate>();
            }
            else if (!setTargetFramerate)
                MonoBehaviour.DestroyImmediate(TFR);
            
            if (TFR != null) TFR.targetFrameRate = targetFrameRate;
        }
    }
}

