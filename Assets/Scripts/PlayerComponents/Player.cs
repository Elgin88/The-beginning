using UnityEngine;
using Assets.Scripts.Movement;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    internal class Player : MonoBehaviour, IMoveable
    {
        private PlayerWallet _wallet;

        public float Speed {get; private set;}

        public float RotationSpeed {get; private set;}

        public PlayerWallet Wallet => _wallet;

        public Transform Transform => transform;

        private void Start()
        {
            _wallet = new PlayerWallet();
        }

        [Inject]
        private void Construct(PlayerData config)
        {
            Speed = config.Speed;
            RotationSpeed = config.RotationSpeed;
        }
    }
}