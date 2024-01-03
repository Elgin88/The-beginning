using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.UnitStateMachine
{
    [RequireComponent(typeof(NextTargetFinder))]

    public class StateMove : State
    {
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

        private void Start()
        {
            _nextTargetFinder = GetComponent<NextTargetFinder>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private IEnumerator Move()
        {
            while (true)
            {
                _navMeshAgent.destination = _nextTargetFinder.NextTarget.transform.position;

                yield return null;
            }
        }
    }
}