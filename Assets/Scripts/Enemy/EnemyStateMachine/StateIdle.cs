using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(TransitionIdle))]

    internal class StateIdle : State
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private EnemyAnimation _enemyAnimation;
        private TransitionIdle _transitionIdle;

        internal override void StartState()
        {
            _transitionIdle.StartCheckTransition();
            _enemyAnimation.StartPlayIdle();
        }

        internal override void StopState()
        {
            _transitionIdle.StopCheckTransition();
            _enemyAnimation.StopPlayIdle();
        }

        internal override State TryGetNextState()
        {
            return _transitionIdle.GetNextState();
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _transitionIdle = GetComponent<TransitionIdle>();
        }
    }
}