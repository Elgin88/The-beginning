using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(Enemy))]

    internal class EnemyRange : Enemy
    {
        internal override NavMeshAgent NavMeshAgent { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        internal override float CurrentSpeed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}