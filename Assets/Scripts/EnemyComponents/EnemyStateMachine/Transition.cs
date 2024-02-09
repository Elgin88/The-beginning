using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal class Transition : MonoBehaviour
    {
        protected State NextState;
        protected bool IsNeedNextState;

        internal void SetTrueIsNeedNextState()
        {
            IsNeedNextState = true;
        }

        internal void SetFalseIsNeedNextState()
        {
            IsNeedNextState = false;
        }

        internal bool GetIsNeedNextState()
        {
            return IsNeedNextState;
        }

        internal State GetNextState()
        {
            return NextState;
        }
    }
}