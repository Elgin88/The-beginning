using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(EnemyAnimation))]
    [RequireComponent(typeof(TransitionIdle))]
    [RequireComponent(typeof(Rigidbody))]

    internal class StateIdle : State
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyAnimation _enemyAnimation;
        [SerializeField] private TransitionIdle _transitionIdle;
        [SerializeField] private Rigidbody _rigidboy;

        internal override void StartState()
        {
            _transitionIdle.StartCheckTransition();
            _enemyAnimation.StartPlayIdle();
            _rigidboy.isKinematic = false;

            if (_enemyNextTargetFinder.CurrentTarget != null)
            {
                transform.rotation = Quaternion.LookRotation(_enemyNextTargetFinder.CurrentTargetPosition, Vector3.up);
            }
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