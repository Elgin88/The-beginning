using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(StateIdle))]

    internal class TransitionMove : Transition
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private EnemyRayPoint _enemyRayPoint;
        private StateIdle _stateIdle;
        private float _minDistanceToTarget = 2.0f;

        protected override Coroutine CheckTransition { get; set; }

        protected override State NextState { get; set; }

        internal override State GetNextState()
        {
            return NextState;
        }

        internal override IEnumerator CheckTransitionIE()
        {
            while (true)
            {
                NextState = null;

                if (IsMinDistanceToPlayerObject() || _enemyNextTargetFinder.CurrentTarget == null)
                {
                    NextState = _stateIdle;
                }

                yield return null;
            }
        }

        internal bool IsMinDistanceToPlayerObject()
        {
            bool isMinDistance = false;

            Ray ray = new Ray(_enemyRayPoint.transform.position, transform.forward);

            if (Physics.Raycast(_enemyRayPoint.transform.position, ray.direction, out RaycastHit raysactHit))
            {
                if (raysactHit.transform.TryGetComponent(out IDamageable idamageable))
                {
                    if (idamageable.IsPlayerObject == true & raysactHit.distance <= _minDistanceToTarget)
                    {
                        isMinDistance = true;
                    }
                }
            }

            return isMinDistance;
        }

        internal override void StartCheckTransition()
        {
            if (CheckTransition == null)
            {
                CheckTransition = StartCoroutine(CheckTransitionIE());
            }
        }

        internal override void StopCheckTransition()
        {
            if (CheckTransition != null)
            {
                CheckTransition = StartCoroutine(CheckTransitionIE());
                CheckTransition = null;
            }
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _stateIdle = GetComponent<StateIdle>();

            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();
        }
    }
}