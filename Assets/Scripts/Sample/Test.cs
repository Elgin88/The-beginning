using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.Movement;
using Assets.Scripts.EnemyComponents;

namespace Assets.Scripts.Sample
{
    internal class Test : MonoBehaviour, IDamageable, IMoveable, IEnemy
    {
        [SerializeField] private float _health;

        public float Speed => 1;

        public float RotationSpeed => 1;

        public Transform Position => transform;

        public Transform Transform => transform;

        public void TakeDamage(float damage)
        {
            _health -= damage;

            Debug.Log(_health);
        }
    }
}
