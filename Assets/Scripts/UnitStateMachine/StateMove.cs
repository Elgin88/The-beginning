using System.Collections;
using UnityEngine;

namespace Scripts.UnitStateMachine
{
    [RequireComponent(typeof(NextTargetFinder))]

    public class StateMove : State
    {
        private float _speed = 2;
        private Coroutine _move;
        private NextTargetFinder _nextTargetFinder;

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
        }

        private IEnumerator Move()
        {
            while (true)
            {
                transform.position = Vector3.MoveTowards(gameObject.transform.position, _nextTargetFinder.NextTarget.transform.position, _speed * Time.deltaTime);

                yield return null;
            }
        }
    }
}