using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]

    internal class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _health;

        public Transform Transform => gameObject.transform;

        public void TakeDamage(int damage)
        {
            _health += damage;

            if (_health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}