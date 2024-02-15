using System;
using System.Collections;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class TransitionAttack : Transition
    {
        protected override State NextState { get; set; }
        protected override Coroutine CheckTransition { get; set; }

        internal override State GetNextState()
        {
            State state = null;

            return state;
        }

        internal override IEnumerator CheckTransitionIE()
        {
            yield return null;
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