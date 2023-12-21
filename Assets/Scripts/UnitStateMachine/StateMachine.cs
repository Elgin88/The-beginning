using System.Collections;
using UnityEngine;

namespace Scripts.UnitStateMachine
{
    [RequireComponent(typeof(NextTargetFinder))]
    [RequireComponent(typeof(StateMove))]
    [RequireComponent(typeof(TransitionMove))]

    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private State _startState;
        [SerializeField] private NextTargetFinder _nextTargetFinder;

        private State _currentState = null;
        private Coroutine _startTrySetNextState;

        public NextTargetFinder NextTargetFinder => _nextTargetFinder;

        private void Start()
        {
            _currentState = _startState;

            StartCurrentState();
            StartTrySetNextState();
        }

        private void StartCurrentState()
        {
            if (_currentState != null)
            {
                _currentState.StartState();
            }
        }

        private void StopCurrentState()
        {
            if (_currentState != null)
            {
                _currentState.StopState();
            }
        }

        private IEnumerator TrySetNextState()
        {
            while (true)
            {
                if (_currentState != null & _currentState.IsNeedSetNextState)
                {
                    StopCurrentState();
                    _currentState = _currentState.GetNextState();
                    StartCurrentState();
                }

                yield return null;
            }
        }

        private void StartTrySetNextState()
        {
            if (_startTrySetNextState == null)
            {
                _startTrySetNextState = StartCoroutine(TrySetNextState());
            }
        }
    }
}