using UnityEngine;
using System;

namespace TerraUnity.Runtime
{
    [Serializable]
    public class TLayerScript : MonoBehaviour
    {
        [HideInInspector] public string unityLayerName;
        [HideInInspector] public int unityLayerIndex;
        private protected Terrain _terrain;
        public bool updatePlacement = false;

        public Terrain terrain
        {
            get
            {
                if (_terrain == null) SetParentTerrain();
                return _terrain;
            }
        }

        private void SetParentTerrain()
        {
            // Recursive finding of terrain object in parent
            Transform parent = transform;
            
            while (parent != null)
            {
                if (parent.GetComponent<Terrain>() != null)
                {
                    _terrain = parent.GetComponent<Terrain>();
                    break;
                }
                else
                    parent = parent.transform.parent;
            }
        }

        protected void Validate()
        {
            if (_terrain == null) SetParentTerrain();
            unityLayerIndex = LayerMask.NameToLayer(unityLayerName);

            if (updatePlacement)
            {
                UpdateLayer();
                updatePlacement = false;
            }
        }

        public virtual void UpdateLayer()
        {
            throw new Exception("UpdateLayer function not implemented!");
        }
    }
}

