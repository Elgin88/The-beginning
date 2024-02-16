using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class State : MonoBehaviour
    {
        internal abstract void StartState();

        internal abstract void StopState();

        internal abstract State TryGetNextState();
    }
}