using Assets.Scripts.UnitStateMachine;

namespace Assets.Scripts.Enemy
{
    internal class TransitionAttack : Transition
    {
        private StateAttack _stateAttack;

        internal void SetNextStateIsAttack()
        {
            NextState = _stateAttack;
        }

        private void Awake()
        {
            _stateAttack = GetComponent<StateAttack>();
        }
    }
}