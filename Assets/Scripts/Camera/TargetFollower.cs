using UnityEngine;
using Zenject;
using Assets.Scripts.PlayerComponents;

namespace Assets.Scripts.Camera
{
    internal class TargetFollower : MonoBehaviour
    {
        private Player _player;

        private Vector3 _cameraOffset;

        private void Awake()
        {
            _cameraOffset = transform.position;
        }

        private void LateUpdate()
        {
            Vector3 newPosition = _player.transform.position + _cameraOffset;
            transform.position = newPosition;
        }

        [Inject]
        private void Construct(Player player)
        {
            _player = player;
        }
    }
}
