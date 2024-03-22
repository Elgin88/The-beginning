using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(Rigidbody))]

    internal class Enemy : MonoBehaviour, IDamageable, IEnemy
    {
        [SerializeField] private float _health;
        [SerializeField] private float _damage;
        [SerializeField] protected NavMeshAgent NavMeshAgent;

        private MainBuilding _mainBuilding;
        private float _currentSpeed;
        private bool _isDead;

        internal MainBuilding MainBuilding => _mainBuilding;

        internal float CurrentSpeed => _currentSpeed;

        public bool IsPlayerObject => false;

        public bool IsDead => _isDead;

        public Transform Transform => transform;

        float IEnemy.Health => _health;

        float IEnemy.Damage => _damage;

        internal void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        internal void SetRotationToTarget(Vector3 targetPosition)
        {
            transform.rotation = Quaternion.LookRotation(targetPosition);
        }

        internal void InitMainBuilding(MainBuilding mainBuilding)
        {
            _mainBuilding = mainBuilding;
        }

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
            _currentSpeed = NavMeshAgent.speed;
        }
    }
}