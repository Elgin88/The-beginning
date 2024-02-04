using Assets.Scripts.UnitStateMachine;

namespace Assets.Scripts.Enemy
{
    internal class TransitionAttack : Transition
    {
        public override State NextState { get; set; }
        public override bool IsNeedNextState { get; set; }
    }
}