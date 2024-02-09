using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]

    internal class EnemyMeleeOgreGreen : EnemyMelee
    {
        private float _startHealth = 100;
        private float _startDamage = 80;

        private void Awake()
        {
            Health = _startHealth;
            Damage = _startDamage;
        }
    }
}