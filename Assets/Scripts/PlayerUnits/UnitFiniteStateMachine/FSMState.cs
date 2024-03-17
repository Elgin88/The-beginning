using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal abstract class FSMState
    {
        protected FiniteStateMachine FSM;
        protected Unit Unit;
        protected UnitData Data;
        protected NavMeshAgent UnitNavMesh;
        protected UnitAnimator Animator;

        public FSMState(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator, UnitData data)
        {
            FSM = fsm;
            Unit = unit;
            UnitNavMesh = navMesh;
            Animator = animator;
            Data = data;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }
}
