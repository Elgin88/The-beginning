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
                Vector3 currentPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
                Vector3 targetPosition = new Vector3(_nextTargetFinder.NextTarget.transform.position.x, gameObject.transform.position.y, _nextTargetFinder.NextTarget.transform.position.z);

                transform.position = Vector3.MoveTowards(currentPosition, targetPosition, _speed * Time.deltaTime);

                yield return null;
            }
        }
    }
}