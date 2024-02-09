using System;
using Assets.Scripts.UnitStateMachine;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(StateMove))]

    internal class TransitionAttack : Transition
    {
        private StateMove _stateMove;

        internal override State GetNextState()
        {
            return NextState;
        }

        private void Awake()
        {
            _stateMove = GetComponent<StateMove>();
        }
    }
}