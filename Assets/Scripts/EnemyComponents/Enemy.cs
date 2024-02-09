using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyVision))]
    [RequireComponent(typeof(Animator))]

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

            if (Health <= 0)
            {
                _isDead = true;
                Destroy(gameObject);
            }
        }

        private void Awake()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();

            CurrentSpeed = NavMeshAgent.speed;
        }
    }
}