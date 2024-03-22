using System.Collections;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;

namespace Assets.Scripts.EnemyNamespace
{
    internal class TransitionAttack : Transition
    {
        [SerializeField] private StateIdle _stateIdle;

        protected override State NextState { get; set; }
        protected override Coroutine CheckTransition { get; set; }

        internal override State GetNextState()
        {
            return NextState;
        }

        internal override IEnumerator CheckTransitionIE()
        {
            yield return null;
        }

        internal override void StartCheckTransition()
        {
            if (CheckTransition == null)
            {
                NextState = null;
                CheckTransition = StartCoroutine(CheckTransitionIE());
            }
        }

        internal override void StopCheckTransition()
        {
            CheckTransition = StartCoroutine(CheckTransitionIE());
            CheckTransition = null;
        }

        internal void SetStateIdle()
        {
            NextState = _stateIdle;
        }
    }
}