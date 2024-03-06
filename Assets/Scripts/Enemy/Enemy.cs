using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]

    internal abstract class Enemy : MonoBehaviour, IDamageable, IEnemy
    {
        [SerializeField] private float _health;
        [SerializeField] private float _damage;

        private bool _isDead;

        protected NavMeshAgent NavMeshAgent;

        protected float CurrentSpeed;

        public bool IsPlayerObject => false;

        public bool IsDead => _isDead;

        public Transform Transform => transform;

        float IEnemy.Health => _health;

        float IEnemy.Damage => _damage;

        void IDamageable.TakeDamage(float damage)
        {
            _health -= damage;

            if (_health <= 0)
            {
                _isDead = true;
                gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();

            CurrentSpeed = NavMeshAgent.speed;
        }
    }
}