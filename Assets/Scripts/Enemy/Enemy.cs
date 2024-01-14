using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NextTargetFinder))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]

    internal class Enemy : MonoBehaviour, IDamageable
    {
        public Transform Transform => gameObject.transform;

        public void TakeDamage(int damage)
        {
            throw new System.NotImplementedException();
        }
    }
}