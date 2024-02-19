using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(TransitionMove))]
    [RequireComponent(typeof(StateAttack))]
    [RequireComponent(typeof(StateMove))]

    internal class TransitionIdle : Transition
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private TransitionMove _transitionMove;
        private StateAttack _stateAttack;
        private StateMove _stateMove;

        protected override Coroutine CheckTransition { get; set; }
        protected override State NextState { get; set; }

        internal override State GetNextState()
        {
            return NextState;
        }

        internal override IEnumerator CheckTransitionIE()
        {
            while (true)
            {
                NextState = null;

                if(_enemyNextTargetFinder.CurrentTarget == null)
                {
                    NextState = null;
                }
                else if (_transitionMove.IsMinDistanceToPlayerObject())
                {
                    NextState = _stateAttack;
                }
                else
                {
                    NextState = _stateMove;
                }

                yield return null;
            }
        }

        internal override void StartCheckTransition()
        {
            if (CheckTransition == null)
            {
                CheckTransition = StartCoroutine(CheckTransitionIE());
            }
        }

        internal override void StopCheckTransition()
        {
            if (CheckTransition != null)
            {
                CheckTransition = StartCoroutine(CheckTransitionIE());
                CheckTransition = null;
            }
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _transitionMove = GetComponent<TransitionMove>();
            _stateAttack = GetComponent<StateAttack>();
            _stateMove = GetComponent<StateMove>();
        }
    }
}