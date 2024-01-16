using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NextTargetFinder))]

    internal class Enemy : MonoBehaviour //, IDamageable
    {
        public Transform Transform => gameObject.transform;

        public void TakeDamage()
        {
            throw new System.NotImplementedException();
        }
    }
}