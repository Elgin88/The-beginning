using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class Transition : MonoBehaviour
    {
        protected abstract State NextState { get; set; }

        internal abstract State GetNextState();
    }
}