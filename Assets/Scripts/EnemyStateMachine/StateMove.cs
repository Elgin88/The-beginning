using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(TransitionMove))]
    [RequireComponent(typeof(EnemyNextTargetFinder))]

    internal class StateMove : State
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private NavMeshAgent _navMeshAgent;
        private TransitionMove _transitionMove;
        private Coroutine _move;
        private StateAttack _stateAttack;
        private bool _isNeedNextState;

        internal override bool IsNeedNextState { get; set; }

        internal override void StartState()
        {
            if (_move == null)
            {
                _move = StartCoroutine(Move());
            }
        }

        internal override void StopState()
        {
            if (_move != null)
            {
                StopCoroutine(_move);
                _move = null;

                _transitionMove.StopCallculateDistance();
            }
        }

        internal override State GetNextState()
        {
            if (_transitionMove.IsNeedAttackState)
            {
                return _stateAttack;
            }

            return null;
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _transitionMove = GetComponent<TransitionMove>();
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
        }

        private IEnumerator Move()
        {
            yield return null;

            _transitionMove.StartCallculateDistance();

            while (true)
            {
                if (_navMeshAgent != null & _enemyNextTargetFinder != null)
                {
                    _navMeshAgent.destination = _enemyNextTargetFinder.CurrentTarget.transform.position;
                    _isNeedNextState = _transitionMove.IsNeedAttackState;
                    IsNeedNextState = _isNeedNextState;
                }

                yield return null;
            }
        }

    }
}