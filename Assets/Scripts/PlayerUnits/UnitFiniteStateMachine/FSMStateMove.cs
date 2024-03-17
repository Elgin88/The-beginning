using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FSMStateMove : FSMState
    {
        public FSMStateMove(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator, UnitData data)
            : base(fsm, unit, navMesh, animator, data)
        {
        }

        public override void Enter()
        {
            UnitNavMesh.speed = Data.Speed;
            UnitNavMesh.SetDestination(FSM.MovePosition);
            Animator.SetMovingBool(true);
        }

        public override void Exit()
        {
            Animator.SetMovingBool(false);
        }

        public override void Update()
        {
            if (UnitNavMesh.pathEndPosition == Unit.transform.position)
            {
                FSM.SetState<FSMStateIdle>();
            }
        }
    }
}
