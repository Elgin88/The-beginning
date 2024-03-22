using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.EnemyNamespace
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(StateMachine))]

    internal abstract class Enemy : MonoBehaviour, IDamageable, IEnemy
    {
        [SerializeField] private float _health;
        [SerializeField] private float _damage;
        [SerializeField] private NavMeshAgent NavMeshAgent;

        private bool _isDead;

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

        internal abstract void SetPosition(Vector3 position);

        internal abstract void SetRotationToTarget(Vector3 targetPosition);

        internal abstract void InitMainBuilding(MainBuilding mainBuilding);

        private void Awake()
        {
            CurrentSpeed = NavMeshAgent.speed;
        }
    }
}