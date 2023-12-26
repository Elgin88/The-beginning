using UnityEngine;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    [RequireComponent(typeof(Rigidbody))]
    internal class Player : MonoBehaviour, IMovable
    {
        private Rigidbody _rigidbody;

        public float Speed {get; private set;}

        public float RotationSpeed {get; private set;}

        public Transform Transform => transform;

        public Rigidbody Rigidbody => _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        [Inject]
        private void Construct(PlayerConfig config)
        {
            Speed = config.Speed;
            RotationSpeed = config.RotationSpeed;
        }
    }
}