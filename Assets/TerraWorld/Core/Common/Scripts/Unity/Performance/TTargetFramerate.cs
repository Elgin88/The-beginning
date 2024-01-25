// Ref.: https://support.unity.com/hc/en-us/articles/360000283043-Target-frame-rate-or-V-Blank-not-set-properly

using UnityEngine;

namespace TerraUnity.Runtime
{
    public class TTargetFramerate : MonoBehaviour
    {
        [Range(30, 120)] public int targetFrameRate = 60;

        void Start()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }
    }
}

