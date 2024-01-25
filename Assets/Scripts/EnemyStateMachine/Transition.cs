using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class Transition : MonoBehaviour
    {
        internal abstract State NextState { get; set; }
        internal abstract bool IsNeedAttackState { get; set; }
    }
}