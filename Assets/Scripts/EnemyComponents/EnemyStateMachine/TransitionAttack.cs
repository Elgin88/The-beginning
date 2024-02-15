using System;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(StateMove))]

    internal class TransitionAttack : Transition
    {
        private StateMove _stateMove;

        protected override State NextState { get; set; }

        internal override State GetNextState()
        {
            State state = null;

            state = NextState;
            NextState = null;

            return state;
        }

        internal void SetNextStateMove()
        {
            Debug.Log("Move");
            NextState = _stateMove;
        }

        private void Awake()
        {
            _stateMove = GetComponent<StateMove>();
        }
    }
}