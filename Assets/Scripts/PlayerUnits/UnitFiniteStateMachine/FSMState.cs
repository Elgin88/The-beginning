using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FSMState
    {
        protected FiniteStateMachine FSM;
        protected Unit Unit;
        protected NavMeshAgent UnitNavMesh;
        protected UnitAnimator Animator;

        public FSMState(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator)
        {
            FSM = fsm;
            Unit = unit;
            UnitNavMesh = navMesh;
            Animator = animator;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }
}
