using System;
using System.Collections;
using Assets.Scripts.Enemy;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(StateAttack))]

    internal class TransitionMove : Transition
    {
        protected override State NextState { get; set; }
        protected override bool IsNeedNextState { get; set; }

        internal override bool GetIsNeedNextState()
        {
            return IsNeedNextState;
        }

        internal override State GetNextState()
        {
            return NextState;
        }

        internal void StartCallculateDistance()
        {

        }

        internal void StopCallculateDistance()
        {

        }
    }
}