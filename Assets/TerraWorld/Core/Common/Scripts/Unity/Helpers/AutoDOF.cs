#if TERRAWORLD_PRO
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;

namespace TerraUnity.Runtime
{
    public class AutoDOF : MonoBehaviour
    {
        [Range(0.1f, 10f)] public float focusSpeed = 5f;
        public float maxFocusDistance = 50f;

        private void Update()
        {
            PostProcessingManager.SetDOFFocus(focusSpeed, transform.position, transform.forward, maxFocusDistance);
        }
    }
}
#endif
#endif

