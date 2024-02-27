﻿using UnityEngine.AI;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FSMStateMove : FSMState
    {
        public FSMStateMove(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator) : base(fsm, unit, navMesh, animator)
        {
        }

        public override void Enter()
        {
            UnitNavMesh.SetDestination(FSM.MovePosition);
            Animator.SetMovingBool(true);
        }

        public override void Exit()
        {
            Animator.SetMovingBool(false);
        }

        public override void Update()
        {
            if (Unit.transform.position == FSM.MovePosition)
            {
                FSM.SetState<FSMStateIdle>();
            }
        }
    }
}
