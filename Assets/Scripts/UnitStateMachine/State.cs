using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal abstract class State : MonoBehaviour
    {
        [SerializeField] private List<Transition> _transitions;

        private bool _isNeedNextState = false;

        public bool IsNeedSetNextState => _isNeedNextState;

        public abstract void StartState();

        public abstract void StopState();

        public abstract void GetNextTransition();

        public abstract State GetNextState();
    }
}