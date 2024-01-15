using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(EnemyAnimationController))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]

    internal abstract class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _health;

        private NavMeshAgent _navMeshAgent;
        private float _currentSpeed;

        Transform IDamageable.Transform => gameObject.transform;

        internal NavMeshAgent NavMeshAgent => _navMeshAgent;

        

        internal float CurrentSpeed => _currentSpeed;

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

            _currentSpeed = _navMeshAgent.speed;
        }
    }
}