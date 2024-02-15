using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(TransitionIdle))]

    internal class StateIdle : State
    {
        private EnemyAnimation _enemyAnimation;
        private TransitionIdle _transitionIdle;
        private Coroutine _idle;

        internal override void StartState()
        {
            if (_idle == null)
            {
                _idle = StartCoroutine(StartIdle());

                _transitionIdle.StartCheckTransition();
            }
        }

        internal override void StopState()
        {
            StopCoroutine(_idle);
            _idle = null;

            _enemyAnimation.StopPlayIdle();
            _transitionIdle.StopCheckTransition();
        }

        internal override State TryGetNextState()
        {
            return _transitionIdle.GetNextState();
        }

        private void Awake()
        {
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _transitionIdle = GetComponent<TransitionIdle>();
        }

        private IEnumerator StartIdle()
        {
            while (true)
            {
                _enemyAnimation.PlayIdle();

                yield return null;
            }
        }
    }
}