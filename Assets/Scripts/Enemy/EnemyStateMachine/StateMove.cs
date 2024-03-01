using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(TransitionMove))]
    [RequireComponent(typeof(NavMeshAgent))]

    internal class StateMove : State
    {
        [SerializeField] private float _maxSpeed;

        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private EnemyAnimation _enemyAnimation;
        private TransitionMove _transitionMove;
        private NavMeshAgent _navMeshAgent;
        private Coroutine _move;

        internal override void StartState()
        {
            if (_move == null)
            {
                _move = StartCoroutine(Move());

                _transitionMove.StartCheckTransition();
                _enemyAnimation.StartPlayRun();
            }
        }

        internal override void StopState()
        {
            StopCoroutine(_move);
            _move = null;

            ResetPath();

            _transitionMove.StopCheckTransition();
            _enemyAnimation.StopPlayRun();
        }

        internal override State TryGetNextState()
        {
            return _transitionMove.GetNextState();
        }

        private void ResetPath()
        {
            _navMeshAgent.ResetPath();
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _transitionMove = GetComponent<TransitionMove>();
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _navMeshAgent.speed = _maxSpeed;
        }

        private IEnumerator Move()
        {
            while (true)
            {
                if (_enemyNextTargetFinder.CurrentTarget != null)
                {
                    _navMeshAgent.SetDestination(_enemyNextTargetFinder.CurrentTarget.transform.position);
                }

                yield return null;
            }
        }
    }
}