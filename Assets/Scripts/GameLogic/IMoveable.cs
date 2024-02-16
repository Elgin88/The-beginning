using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    internal interface IMoveable
    {
        public Transform Transform { get; }

        public float MoveSpeed { get; }

        public float RotationSpeed { get; }

        public SurfaceAlignment SurfaceAlignment { get; }
    }
}
