#if UNITY_EDITOR
using UnityEngine;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    [ExecuteAlways]
    public class GrabEditorCamMatrix : MonoBehaviour
    {
        void Update()
        {
            if (Application.isPlaying || TCameraManager.SceneCamera == null) return;
            transform.position = TCameraManager.SceneCamera.transform.position;
            transform.rotation = TCameraManager.SceneCamera.transform.rotation;
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
#endif
        }

#if UNITY_EDITOR
        protected virtual void OnEditorUpdate()
        {
            Update();
        }
#endif
    }
#endif
}
#endif

