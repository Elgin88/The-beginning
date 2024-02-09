using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class Transition : MonoBehaviour
    {
        protected State NextState;

        internal abstract State GetNextState();
    }
}