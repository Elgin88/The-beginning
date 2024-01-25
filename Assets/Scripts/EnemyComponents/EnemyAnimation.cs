using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]

    internal class EnemyAnimation : MonoBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private string _orcBossRunForwardSpeed = "OrcBossRunForwardSpeed";
        private string _run = "Run";
        private string _attack = "Attack";
        private float _baseMoveSpeed = 3;
        private float _normalizeMoveSpeed;
        private float _normalizeAttackSpeed;

        internal void Run()
        {
            _animator.SetBool(_run, true);
        }

        internal void Attack()
        {
            _animator.SetBool(_attack, true);
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            _normalizeMoveSpeed = _navMeshAgent.speed / _baseMoveSpeed;

            _animator.SetFloat(_orcBossRunForwardSpeed, _normalizeMoveSpeed);
        }


    }
}