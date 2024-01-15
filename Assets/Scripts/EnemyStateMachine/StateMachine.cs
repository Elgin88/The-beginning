using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(StateMove))]
    [RequireComponent(typeof(TransitionMove))]

    internal class StateMachine : MonoBehaviour
    {
        [SerializeField] private State _startState;

        private State _currentState;
        private Coroutine _startTrySetNextState;

        private IEnumerator Start()
        {
            yield return null;

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
                if (_currentState != null & _currentState.IsNeedNextState)
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