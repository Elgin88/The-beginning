using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class Transition : MonoBehaviour
    {
        public abstract State NextState { get; set; }
        public abstract bool IsNeedNextState { get; set; }
    }
}