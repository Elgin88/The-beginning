using Assets.Scripts.Static;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Enemy))]

    internal class EnemyAnimation : MonoBehaviour
    {
        [SerializeField] private float _baseMoveSpeed = 3;

        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private float _normalizeSpeed;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            _normalizeSpeed = _navMeshAgent.speed / _baseMoveSpeed;

            _animator.SetFloat(EnemyAnimatorParameters.OrcBossRunForwardSpeedMultiplier, _normalizeSpeed);
        }
    }
}