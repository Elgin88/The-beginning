using Assets.Scripts.EnemyNamespace;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(Rigidbody))]

    internal class StateIdle : State
    {
        [SerializeField] private EnemyVision _enemyVision;
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyAnimation _enemyAnimation;
        [SerializeField] private TransitionIdle _transitionIdle;
        [SerializeField] private Rigidbody _rigidboy;
        [SerializeField] private Enemy _enemy;

        internal override void StartState()
        {
            _transitionIdle.StartCheckTransition();
            _enemyAnimation.StartPlayIdle();
            _enemyVision.StartVision();
            _rigidboy.isKinematic = false;
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
    }
}