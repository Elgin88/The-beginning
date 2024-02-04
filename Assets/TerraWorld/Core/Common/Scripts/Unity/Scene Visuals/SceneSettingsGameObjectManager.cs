using UnityEngine;
using TerraUnity.Edittime;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class SceneSettingsGameObjectManager : MonoBehaviour
    {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
        //private void OnEnable()
        //{
        //    if (Application.isPlaying) return;
        //    SceneSettingsManager.EnableVisualFX();
        //    Debug.Log("Enabled VFX");
        //}
        //
        //private void OnDisable()
        //{
        //    if (Application.isPlaying) return;
        //    SceneSettingsManager.DisableVisualFX();
        //    Debug.Log("Disabled VFX");
        //}
        private void OnDestroy()
        {
            SceneSettingsManager.Destroyed();
        }
#endif
#endif
    }
}

