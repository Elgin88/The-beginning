using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class Transition : MonoBehaviour
    {
        protected abstract Coroutine CheckTransition { get; set; }

        protected abstract State NextState { get; set; }

        internal abstract State GetNextState();

        internal abstract void StartCheckTransition();

        internal abstract void StopCheckTransition();

        internal abstract IEnumerator CheckTransitionIE();
    }
}