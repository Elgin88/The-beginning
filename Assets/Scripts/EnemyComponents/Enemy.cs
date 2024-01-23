using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyVision))]
    [RequireComponent(typeof(Animator))]

    internal abstract class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _health;

        public Transform Transform => transform;

        public bool IsPlayerObject => false;

        internal abstract NavMeshAgent NavMeshAgent { get; set; }

        internal abstract float CurrentSpeed { get; set; }

        void IDamageable.TakeDamage(float damage)
        {
            _health -= damage;

            if (_health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}