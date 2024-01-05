using UnityEngine;

namespace Assets.Scripts.Movement
{
    internal interface IMoveable
    {
        public float Speed { get; }

        public float RotationSpeed { get; }

        public Transform Transform { get; }
    }
}
