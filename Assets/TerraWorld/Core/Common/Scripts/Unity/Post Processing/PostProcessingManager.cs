#if TERRAWORLD_PRO
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace TerraUnity.Runtime
{
    public struct PostProcessingParams
    {
        public PostProcessLayer.Antialiasing antiAliasingType;

        [XmlIgnore] public PostProcessProfile postProcessProfile;
#if UNITY_EDITOR
        public string PostProcessProfilePath { get => AssetDatabase.GetAssetPath(postProcessProfile); set => postProcessProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(value); }
#endif
    }

    public class PostProcessingManager
    {
        public static string postProcessingProfileName = "PostProcessing Profile.asset";

#if UNITY_POST_PROCESSING_STACK_V2
        public static PostProcessVolume volume = null;
        public static DepthOfField depthOfField = null;
        
        private static Ray raycast;
        private static RaycastHit hit;
        private static float hitDistance;
#endif

#if UNITY_POST_PROCESSING_STACK_V2
        public static PostProcessLayer PostProcessLayer { get => GetPostProcessLayer(); }
        private static PostProcessLayer _postProcessLayer;

        public static PostProcessVolume PostProcessVolumeScript { get => GetPostProcessVolumeScript(); }
        private static PostProcessVolume _postProcessVolumeScript;


#if UNITY_EDITOR
        private static string ProfileLocalPath { get => TTerraWorldManager.WorkDirectoryLocalPath + postProcessingProfileName; }
#endif

        private static PostProcessLayer GetPostProcessLayer()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return null;

            if (_postProcessLayer == null)
            {
                if (TCameraManager.MainCamera != null)
                {
                    _postProcessLayer = TCameraManager.MainCamera.GetComponent<PostProcessLayer>();

                    //if (_postProcessLayer == null)
                    //    _postProcessLayer = TCameraManager.MainCamera.gameObject.AddComponent<PostProcessLayer>();
                    //
                    //_postProcessLayer.volumeLayer = LayerMask.GetMask("TransparentFX");
                    //_postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                    //PostProcessVolumeScript.enabled = true;
                }
            }

            return _postProcessLayer;
        }

        private static PostProcessingParams _parameters;

        public static PostProcessingParams GetParams()
        {
            _parameters.antiAliasingType = PostProcessLayer.antialiasingMode;
            _parameters.postProcessProfile = PostProcessVolumeScript.profile;
            return _parameters;
        }

        public static void SetParams(PostProcessingParams parameters)
        {
            _parameters = parameters;
            PostProcessLayer.antialiasingMode = parameters.antiAliasingType;
        }

        private static PostProcessVolume GetPostProcessVolumeScript()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return null;

            if (_postProcessVolumeScript == null)
            {
                foreach (Transform t in TTerraWorldManager.IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    PostProcessVolume script = t.GetComponent<PostProcessVolume>();

                    if (script != null)
                    {
                        _postProcessVolumeScript = script;
                        break;
                    }
                }

                if (_postProcessVolumeScript == null)
                {
                    if (TTerraWorldManager.SceneSettingsGO1 != null)
                    {
                        _postProcessVolumeScript = TTerraWorldManager.SceneSettingsGO1.GetComponent<PostProcessVolume>();

                        if (_postProcessVolumeScript == null)
                        {
                            _postProcessVolumeScript = TTerraWorldManager.SceneSettingsGO1.AddComponent<PostProcessVolume>();
                            _postProcessVolumeScript.isGlobal = true;
                        }
                    }
                }
            }

            if (_postProcessVolumeScript != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && _postProcessVolumeScript.sharedProfile == null)
                {
                    if (!File.Exists(ProfileLocalPath))
                    {
                        TResourcesManager.LoadPostProcessingResources();
                        PostProcessProfile postProcessingAsset = TResourcesManager.postProcessProfile;
                        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(postProcessingAsset), ProfileLocalPath);
                    }
                            
                    _postProcessVolumeScript.sharedProfile = AssetDatabase.LoadAssetAtPath(ProfileLocalPath, typeof(PostProcessProfile)) as PostProcessProfile;
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }
#endif
            }

            if (_postProcessVolumeScript != null && !_postProcessVolumeScript.isGlobal)
                _postProcessVolumeScript.isGlobal = true;

            return _postProcessVolumeScript;
        }
#endif

#if UNITY_EDITOR
        public static PostProcessProfile SharedProfile { get => PostProcessVolumeScript.sharedProfile; set => SetSharedProfile(value); }

        private static void SetSharedProfile(PostProcessProfile newPostProcessProfile)
        {
            if (PostProcessVolumeScript.sharedProfile != newPostProcessProfile)
            {
                if (File.Exists(ProfileLocalPath))
                    File.Delete(ProfileLocalPath);

                if (newPostProcessProfile != null)
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(newPostProcessProfile), ProfileLocalPath);

                PostProcessVolumeScript.sharedProfile = null;
            }
        }
#endif

        public static void SetDOFFocus (float focusSpeed, Vector3 position, Vector3 forward, float maxFocusDistance)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (volume == null) volume = PostProcessVolumeScript;
            if (depthOfField == null) volume.profile.TryGetSettings(out depthOfField);
            if (volume == null || depthOfField == null) return;

            raycast = new Ray(position, forward * maxFocusDistance);

            if (Physics.Raycast(raycast, out hit, maxFocusDistance))
                hitDistance = Vector3.Distance(position, hit.point);
            else if (hitDistance < maxFocusDistance)
                hitDistance++;

            depthOfField.focusDistance.value = Mathf.Lerp(depthOfField.focusDistance.value, hitDistance, Time.deltaTime * focusSpeed);
#endif
        }

        public static bool IsPostProcessing()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (PostProcessLayer != null) 
                return PostProcessLayer.enabled;
            else
                return false;
#else
            return false;
#endif
        }

        public static void SetPostProcessing(bool enabled)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (enabled)
            {
                if (PostProcessLayer == null)
                {
                    _postProcessLayer = TCameraManager.MainCamera.gameObject.AddComponent<PostProcessLayer>();

                    _postProcessLayer.volumeLayer = LayerMask.GetMask("TransparentFX");
                    _postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                    PostProcessVolumeScript.enabled = true;
                }
                PostProcessLayer.enabled = true;
            }
            else
                if (PostProcessLayer != null)
                    PostProcessLayer.enabled = false;
#endif
        }
    }
}
#endif
#endif

