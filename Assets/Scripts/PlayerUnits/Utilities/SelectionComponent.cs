using UnityEngine;

namespace Assets.Scripts.PlayerUnits.Utilities
{
    internal class SelectionComponent : MonoBehaviour
    {
        private Renderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();

            _renderer.material.color = Color.red;
        }

        private void OnDestroy()
        {
            _renderer.material.color = Color.white;
        }
    }
}
