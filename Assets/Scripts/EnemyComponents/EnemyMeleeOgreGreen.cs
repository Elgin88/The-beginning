using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]

    internal class EnemyMeleeOgreGreen : EnemyMelee
    {
        internal override NavMeshAgent NavMeshAgent { get; set; }
        internal override float CurrentSpeed { get; set; }

        private void Awake()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();

            CurrentSpeed = NavMeshAgent.speed;
        }
    }
}