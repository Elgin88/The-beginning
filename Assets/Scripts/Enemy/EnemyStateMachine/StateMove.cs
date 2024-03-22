using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.UnitStateMachine
{
    internal class StateMove : State
    {
        [SerializeField] private float _maxSpeed;
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyAnimation _enemyAnimation;
        [SerializeField] private TransitionMove _transitionMove;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        private Coroutine _move;

        internal override void StartState()
        {
            _move = StartCoroutine(Move());
            _transitionMove.StartCheckTransition();
            _enemyAnimation.StartPlayRun();
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
            _navMeshAgent.speed = _maxSpeed;
        }

        private IEnumerator Move()
        {
            while (true)
            {
                if (_enemyNextTargetFinder != null)
                {
                    if (_enemyNextTargetFinder.CurrentTargetPosition != null)
                    {
                        _navMeshAgent.SetDestination(_enemyNextTargetFinder.CurrentTargetPosition);
                    }
                }

                yield return null;
            }
        }
    }
}