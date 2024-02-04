using UnityEngine;

namespace TerraUnity.Runtime
{
    /// <summary>
    /// When attached to a <see cref="GameObject"/>, will trigger grass bending while enabled.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BendGrassWhenEnabled : MonoBehaviourGrassBender
    {
        private void OnEnable ()
        {
            GrassBendingManager.AddBender(this);
        }

        private void OnDisable ()
        {
            GrassBendingManager.RemoveBender(this);
        }

        private void OnDestroy()
        {
            GrassBendingManager.RemoveBender(this);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, BendRadius);
        }
    }
}

