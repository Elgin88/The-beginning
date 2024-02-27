using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FSMStateAttack : FSMState
    {
        public FSMStateAttack(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator) : base(fsm, unit, navMesh, animator)
        {
        }

        public override void Enter()
        {
            
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update()
        {

        }
    }
}
