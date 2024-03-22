using Assets.Scripts.EnemyNamespace;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(Rigidbody))]

    internal class StateIdle : State
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyAnimation _enemyAnimation;
        [SerializeField] private TransitionIdle _transitionIdle;
        [SerializeField] private Rigidbody _rigidboy;
        [SerializeField] private Enemy _enemy;

        private void OnEnable()
        {
            if (_enemyNextTargetFinder != null)
            {
                _enemy.SetRotationToTarget(_enemyNextTargetFinder.CurrentTargetPosition);
            }
        }

        internal override void StartState()
        {
            _transitionIdle.StartCheckTransition();
            _enemyAnimation.StartPlayIdle();
            _rigidboy.isKinematic = false;
        }

        internal override void StopState()
        {
            _transitionIdle.StopCheckTransition();
            _enemyAnimation.StopPlayIdle();
            _rigidboy.isKinematic = true;
        }

        internal override State TryGetNextState()
        {
            return _transitionIdle.GetNextState();
        }
    }
}