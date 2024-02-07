using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(TransitionMove))]
    [RequireComponent(typeof(NavMeshAgent))]

    internal class StateMove : State
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private EnemyAnimation _enemyAnimation;
        private TransitionMove _transitionMove;
        private NavMeshAgent _navMeshAgent;
        private StateAttack _stateAttack;
        private Coroutine _move;

        internal override bool IsNeedNextState { get; set; }

        internal override void StartState()
        {
            if (_move == null)
            {
                _move = StartCoroutine(Move());
                _transitionMove.StartCallculateDistance();
                _enemyAnimation.PlayRun();
            }
        }

        internal override void StopState()
        {
            if (_move != null)
            {
                StopCoroutine(_move);
                _enemyAnimation.StopPlayRun();
                _transitionMove.StopCallculateDistance();

                _move = null;
            }
        }

        internal override State GetNextState()
        {
            if (_transitionMove.GetIsNeedNextState())
            {
                return _transitionMove.GetNextState();
            }

            return null;
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _transitionMove = GetComponent<TransitionMove>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
        }

        private IEnumerator Move()
        {
            while (IsNeedNextState == false)
            {
                _navMeshAgent.destination = _enemyNextTargetFinder.CurrentTarget.transform.position;

                IsNeedNextState = _transitionMove.GetIsNeedNextState();

                if (IsNeedNextState)
                {
                    _navMeshAgent.ResetPath();
                }

                yield return null;
            }
        }
    }
}