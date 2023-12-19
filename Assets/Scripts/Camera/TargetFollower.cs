using UnityEngine;

namespace Assets.Scripts.Camera
{
    internal class TargetFollower : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;

        private Vector3 _cameraOffset;

        private void Awake()
        {
            _cameraOffset = transform.position;
        }

        private void LateUpdate()
        {
            Vector3 newPosition = _targetTransform.position + _cameraOffset;
            transform.position = newPosition;
        }
    }
}
