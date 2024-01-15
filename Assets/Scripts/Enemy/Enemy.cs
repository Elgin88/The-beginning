using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]

    internal class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _health;

        private Animator _animator;
        private float _currentSpeed;
        private NavMeshAgent _navMeshAgent;

        Transform IDamageable.Transform => gameObject.transform;

        public float CurrentSpeed => _currentSpeed;

        void IDamageable.TakeDamage(int damage)
        {
            _health -= damage;

            if (_health <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            _currentSpeed = _navMeshAgent.speed;
        }
    }
}