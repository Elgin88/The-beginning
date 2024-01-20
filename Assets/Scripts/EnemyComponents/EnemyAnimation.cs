using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]

    internal class EnemyAnimation : MonoBehaviour
    {
        private static string _orcBossRunForwardSpeed = "OrcBossRunForwardSpeed";
        private float _baseMoveSpeed = 3;
        private float _normalizeMoveSpeed;
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            _normalizeMoveSpeed = _navMeshAgent.speed / _baseMoveSpeed;

            _animator.SetFloat(_orcBossRunForwardSpeed, _normalizeMoveSpeed);
        }
    }
}