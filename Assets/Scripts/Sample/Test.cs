using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.Movement;

namespace Assets.Scripts.Sample
{
    internal class Test : MonoBehaviour, IDamageable, IMoveable
    {
        public Transform Transform => transform;

        public float Speed => 1;

        public float RotationSpeed => 1;

        public void TakeDamage()
        {
        }
    }
}
