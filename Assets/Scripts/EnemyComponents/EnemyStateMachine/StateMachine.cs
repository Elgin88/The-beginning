using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(StateMove))]

    internal class StateMachine : MonoBehaviour
    {
        private Coroutine _startTrySetNextState;
        private State _currentState;
        private State _startState;

        private void Awake()
        {
            _startState = GetComponent<StateMove>();

            _currentState = _startState;
        }

        private void Start()
        {
            StartCurrentState();
            StartTrySetNextState();
        }

        private IEnumerator TrySetNextState()
        {
            while (true)
            {
                StopCurrentState();

                State nextState = _currentState.GetNextState();

                _currentState = nextState;

                StartCurrentState();

                yield return null;
            }
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

        private void StartTrySetNextState()
        {
            if (_startTrySetNextState == null)
            {
                _startTrySetNextState = StartCoroutine(TrySetNextState());
            }
        }
    }
}