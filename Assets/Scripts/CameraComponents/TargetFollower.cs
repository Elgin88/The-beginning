using UnityEngine;
using Zenject;
using Assets.Scripts.PlayerComponents;

namespace Assets.Scripts.CameraComponents
{
    internal class TargetFollower : MonoBehaviour
    {
        [Header("Position")]
        
        [SerializeField] private float _offsetPositionY;
        [SerializeField] private float _offsetPositionX;
        [SerializeField] private float _offsetPositionZ;

        [Header("Rotation")]

        [SerializeField] private float _offsetRotationY;
        [SerializeField] private float _offsetRotationX;
        [SerializeField] private float _offsetRotationZ;

        private Player _player;
        private int QuaternionWValue = 15;

        [Inject]
        private void Construct(Player player)
        {
            _player = player;
        }
       
        private void Start()
        {
            //SetRotation();
        }

        private void LateUpdate()
        {
            SetPosition();
            SetRotation();
        }


        private void SetPosition()
        {
            transform.position = new Vector3(_player.transform.position.x + _offsetPositionX,
                _player.transform.position.y + _offsetPositionY, _player.transform.position.z + _offsetPositionZ);
        }

        private void SetRotation()
        {
            transform.rotation = new Quaternion(_offsetRotationX, _offsetRotationY, _offsetRotationZ, QuaternionWValue);
        }
    }
}
