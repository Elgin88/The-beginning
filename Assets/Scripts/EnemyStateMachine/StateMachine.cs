using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(StateMove))]
    [RequireComponent(typeof(TransitionMove))]

    internal class StateMachine : MonoBehaviour
    {
        private Coroutine _startTrySetNextState;
        private State _currentState;
        private State _startState;

        private void Awake()
        {
            _startState = GetComponent<StateMove>();
        }

        private void Start()
        {
            _currentState = _startState;

            StartCurrentState();
            StartTrySetNextState();
        }

        private void StartCurrentState()
        {
            _currentState.StartState();
        }

        private void StopCurrentState()
        {
            _currentState.StopState();
        }

        private IEnumerator TrySetNextState()
        {
            yield return null;

            while (true)
            {
                if (_currentState.IsNeedNextState)
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