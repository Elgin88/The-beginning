using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]

    internal class TransitionMove : Transition
    {
        [SerializeField] private State _nextState;

        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private Coroutine _calculateDistance;
        private float _currentDistanceToTarget;
        private float _minDistanceToTarget = 2;

        internal override State NextState { get ; set ; }

        internal override bool IsNeedAttackState { get ; set; }

        public void StartCallculateDistance()
        {
            _calculateDistance = StartCoroutine(CalculateDistance());
        }

        public void StopCallculateDistance()
        {
            StopCoroutine(_calculateDistance);
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();

            StartCallculateDistance();
        }

        private IEnumerator CalculateDistance()
        {
            yield return null;

            SetDistanceToTarget();

            while (_currentDistanceToTarget > _minDistanceToTarget)
            {
                SetDistanceToTarget();

                yield return null;
            }

            IsNeedAttackState = true;

            NextState = _nextState;

            StopCallculateDistance();
        }

        private void SetDistanceToTarget()
        {
            _currentDistanceToTarget = Vector3.Distance(gameObject.transform.position, _enemyNextTargetFinder.CurrentTarget.transform.position);
        }
    }
}