﻿using UnityEngine;

namespace TerraUnity.Runtime
{
    /// <summary>
    /// When attached to a <see cref="GameObject"/> with a <see cref="Renderer"/> component,
    /// will trigger grass bending while rendered by any camera on the scene.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer)), DisallowMultipleComponent]
    public class BendGrassWhenVisible : MonoBehaviourGrassBender
    {
        private void OnBecameVisible ()
        {
            GrassBendingManager.AddBender(this);
        }

        private void OnBecameInvisible ()
        {
            GrassBendingManager.RemoveBender(this);
        }
    }
}
