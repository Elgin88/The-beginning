using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]

    internal class Enemy : MonoBehaviour, IDamageable, IEnemy
    {
        private bool _isDead;

        protected float Health;
        protected float Damage;

        protected NavMeshAgent NavMeshAgent;

        protected float CurrentSpeed;

        public bool IsPlayerObject => false;

        public bool IsDead => _isDead;

        public Transform Transform => transform;

        float IEnemy.Health => Health;

        float IEnemy.Damage => Damage;

        void IDamageable.TakeDamage(float damage)
        {
            Health -= damage;

            Debug.Log(Health);

            if (Health <= 0)
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