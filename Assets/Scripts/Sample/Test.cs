using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.Sample
{
    internal class Test : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _health;

        public float Speed => 1;

        public float RotationSpeed => 1;

        public Transform Transform => transform;

        public bool IsPlayerObject => false;

        public bool IsDead => _health <= 0;

        public void TakeDamage(float damage)
        {
            _health -= damage;

            if (_health <= 0) 
            { 
                gameObject.SetActive(false);
            }
        }
    }
}
