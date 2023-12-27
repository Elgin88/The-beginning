using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal interface IMovable
    {
        public float Speed { get; }

        public float RotationSpeed { get; }

        public Transform Transform { get; }
    }
}
