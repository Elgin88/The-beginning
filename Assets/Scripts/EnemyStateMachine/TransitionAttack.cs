using Assets.Scripts.UnitStateMachine;

namespace Assets.Scripts.Enemy
{
    internal class TransitionAttack : Transition
    {
        internal override State NextState { get; set; }
        internal override bool IsNeedAttackState { get; set; }
    }
}