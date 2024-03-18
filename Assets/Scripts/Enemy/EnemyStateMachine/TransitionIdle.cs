using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal class TransitionIdle : Transition
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private TransitionMove _transitionMove;
        [SerializeField] private StateAttack _stateAttack;
        [SerializeField] private StateMove _stateMove;

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
    }
}