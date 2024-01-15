using System.Collections;
using Assets.Scripts.Bildings;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(Animator))]

    internal class StateMove : State
    {
        [Inject] private PlayerMainBilding _playerMainBilding;

        private Coroutine _move;
        private NavMeshAgent _navMeshAgent;
        private Vector3 _currentTargetPosition;
        private Vector3 _startTargetPosition;

        public override void StartState()
        {
            if (_move == null)
            {
                _move = StartCoroutine(Move());
            }
        }

        public override void StopState()
        {
            if (_move != null)
            {
                StopCoroutine(_move);
                _move = null;
            }
        }

        public override State GetNextState()
        {
            return null;
        }

        public override void GetNextTransition()
        {
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _startTargetPosition = _playerMainBilding.transform.position;
            _currentTargetPosition = _startTargetPosition;
        }

        private IEnumerator Move()
        {
            while (true)
            {
                _navMeshAgent.destination = _currentTargetPosition;

                yield return null;
            }
        }
    }
}