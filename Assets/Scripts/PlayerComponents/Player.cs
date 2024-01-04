using UnityEngine;
using Assets.Scripts.Movement;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    internal class Player : MonoBehaviour, IMoveable
    {
        public float Speed {get; private set;}

        public float RotationSpeed {get; private set;}

        public Transform Transform => transform;

        [Inject]
        private void Construct(PlayerConfig config)
        {
            Speed = config.Speed;
            RotationSpeed = config.RotationSpeed;
        }
    }
}