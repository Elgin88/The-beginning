using Assets.Scripts.UnitStateMachine;

namespace Assets.Scripts.Enemy
{
    internal class TransitionAttack : Transition
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
    }
}