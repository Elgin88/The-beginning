using Assets.Scripts.EnemyComponents;
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

    internal abstract class Enemy : MonoBehaviour, IDamageable, IEnemy
    {
        [SerializeField] private float _health;

        public Transform Position => transform;

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