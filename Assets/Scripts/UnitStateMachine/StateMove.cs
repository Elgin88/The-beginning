using System.Collections;
using UnityEngine;

namespace Scripts.UnitStateMachine
{
    [RequireComponent(typeof(StateMachine))]

    public class StateMove : State
    {
        [SerializeField] private float _speed;
        [SerializeField] private GameObject _playerBilding;

        private Coroutine _move;

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

        private IEnumerator Move()
        {
            while (true)
            {
                transform.position = Vector3.MoveTowards(gameObject.transform.position, _playerBilding.transform.position, _speed * Time.deltaTime);

                yield return null;
            }
        }
    }
}