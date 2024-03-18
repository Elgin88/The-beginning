using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal class StateMachine : MonoBehaviour
    {
        [SerializeField] private StateMove _stateMove;

        private Coroutine _startTrySetNextState;
        private State _currentState;
        private State _startState;

        private void Awake()
        {
            _startState = _stateMove;
            _currentState = _startState;
        }

        private void Start()
        {
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
            _currentState.StopState();
            _currentState = null;
        }


        private void StartTrySetNextState()
        {
            if (_startTrySetNextState == null)
            {
                _startTrySetNextState = StartCoroutine(TrySetNextState());
            }
        }

        private IEnumerator TrySetNextState()
        {
            while (true)
            {
                State nextState = _currentState.TryGetNextState();

                if (nextState != null)
                {
                    StopCurrentState();

                    _currentState = nextState;

                    StartCurrentState();
                }

                yield return null;
            }
        }
    }
}