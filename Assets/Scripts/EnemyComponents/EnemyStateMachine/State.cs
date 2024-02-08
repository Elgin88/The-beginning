using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class State : MonoBehaviour
    {
        protected abstract bool IsNeedNextState { get; set; }

        internal abstract void StartState();

        internal abstract void StopState();

        internal abstract State GetNextState();

        internal abstract bool GetIsNeedNextState();
    }
}