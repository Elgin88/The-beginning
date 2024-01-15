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

        Transform IDamageable.Transform => gameObject.transform;

        internal abstract NavMeshAgent NavMeshAgent { get; set; }

        internal abstract float CurrentSpeed { get; set; }

        void IDamageable.TakeDamage(int damage)
        {
            _health -= damage;

            if (_health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}