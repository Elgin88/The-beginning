using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.EnemyNamespace
{
    internal class EnemyAnimation : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Animator _animator;

        private string _orcBossRunForwardSpeed = "RunForwardSpeed";
        private string _attack = "IsAttack1";
        private string _idle = "IsIdle";
        private string _run = "IsRun";
        private float _normalizeMoveSpeed;
        private float _baseMoveSpeed = 3;

        internal void StartPlayRun()
        {
            _animator.SetBool(_run, true);
        }

        internal void StartPlayAttack()
        {
            _animator.SetBool(_attack, true);
        }

        internal void StartPlayIdle()
        {
            _animator.SetBool(_idle, true);
        }

        internal void StopPlayRun()
        {
            _animator.SetBool(_run, false);
        }

        internal void StopPlayAttack()
        {
            _animator.SetBool(_attack, false);
        }

        internal void StopPlayIdle()
        {
            _animator.SetBool(_idle, false);
        }

        private void Awake()
        {
            _normalizeMoveSpeed = _navMeshAgent.speed / _baseMoveSpeed;
            _animator.SetFloat(_orcBossRunForwardSpeed, _normalizeMoveSpeed);
        }
    }
}