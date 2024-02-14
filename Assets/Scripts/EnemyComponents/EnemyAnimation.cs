using System.Collections;
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
        private string _run = "IsRun";
        private string _attack = "IsAttack1";
        private float _baseMoveSpeed = 3;
        private float _normalizeMoveSpeed;
        private float _normalizeAttackSpeed;

        internal void PlayRun()
        {
            _animator.SetBool(_run, true);
        }

        internal void StopPlayRun()
        {
            _animator.SetBool(_run, false);
        }

        internal void PlayAttack()
        {
            _animator.SetBool(_attack, true);
        }

        internal void StopPlayAttack()
        {
            _animator.SetBool(_attack, false);
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