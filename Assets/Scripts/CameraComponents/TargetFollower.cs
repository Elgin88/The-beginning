using UnityEngine;
using Zenject;
using Assets.Scripts.PlayerComponents;

namespace Assets.Scripts.CameraComponents
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
            Vector3 newPosition = new Vector3(_player.transform.position.x, 0, _player.transform.position.z) + _cameraOffset;
            transform.position = newPosition;
        }

        [Inject]
        private void Construct(Player player)
        {
            _player = player;
        }
    }
}
