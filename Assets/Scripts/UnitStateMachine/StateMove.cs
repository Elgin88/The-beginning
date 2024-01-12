using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Scripts.Static;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(NextTargetFinder))]
    [RequireComponent(typeof(Animator))]

    internal class StateMove : State
    {
        private Animator _animator;
        private Coroutine _move;
        private NextTargetFinder _nextTargetFinder;
        private NavMeshAgent _navMeshAgent;

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
            _animator = GetComponent<Animator>();
            _nextTargetFinder = GetComponent<NextTargetFinder>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private IEnumerator Move()
        {
            while (true)
            {
                _navMeshAgent.destination = _nextTargetFinder.PlayerMainBilding.transform.position;

                yield return null;
            }
        }
    }
}