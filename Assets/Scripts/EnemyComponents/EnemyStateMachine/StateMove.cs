using System;
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
                _transitionMove.StartCallculateDistance();
                _enemyAnimation.PlayRun();
            }
        }

        internal override void StopState()
        {
            if (_move != null)
            {
                StopCoroutine(_move);
                _transitionMove.StopCallculateDistance();
                _enemyAnimation.StopPlayRun();
                _move = null;
            }
        }

        internal override State GetNextState()
        {
            return _transitionMove.GetNextState();
        }


        internal override bool GetIsNeedNextState()
        {
            return _transitionMove.GetIsNeedNextState();
        }

        internal void ResetPath()
        {
            _navMeshAgent.ResetPath();
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _transitionMove = GetComponent<TransitionMove>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private IEnumerator Move()
        {
            while (_transitionMove.GetIsNeedNextState() == false)
            {
                _navMeshAgent.destination = _enemyNextTargetFinder.CurrentTarget.transform.position;

                yield return null;
            }
        }
    }
}