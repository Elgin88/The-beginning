using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class Transition : MonoBehaviour
    {
        protected abstract State NextState { get; set; }
        protected abstract bool IsNeedNextState { get; set; }

        internal abstract bool GetIsNeedNextState();
        internal abstract State GetNextState();
    }
}