using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal class StateMachine : MonoBehaviour
    {
        [SerializeField] private StateMove _stateMove;

        private State _currentState;
        private State _startState;
        private State _nextState;

        private void Awake()
        {
            _startState = _stateMove;
            _currentState = _startState;
        }

        private void Start()
        {
            StartCurrentState();
        }

        private void StartCurrentState()
        {
            _currentState.StartState();
        }

        private void StopCurrentState()
        {
            _currentState.StopState();
        }

        private void Update()
        {
            _nextState = _currentState.TryGetNextState();

            if (_nextState != null)
            {
                StopCurrentState();

                _currentState = _nextState;

                StartCurrentState();
            }
        }
    }
}