using System.Collections;
using Assets.Scripts.Enemy;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal class TransitionMove : Transition
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyRayPoint _enemyRayPoint;
        [SerializeField] private StateIdle _stateIdle;

        private float _minDistanceToTargetForMelleeEnemy = 1.5f;
        private float _minDistanceToTargetForRangeEnemy = 8f;
        private float _minDistanteToTarget;

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

            if (Physics.Raycast(_enemyRayPoint.transform.position, transform.forward, out RaycastHit raysactHit, _minDistanteToTarget))
            {
                if (raysactHit.transform.TryGetComponent(out IDamageable idamageable))
                {
                    if (idamageable.IsPlayerObject == true)
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
            if (TryGetComponent(out EnemyMelee enemyMelee))
            {
                _minDistanteToTarget = _minDistanceToTargetForMelleeEnemy;
            }
            else if (TryGetComponent(out EnemyRange enemyRange))
            {
                _minDistanteToTarget = _minDistanceToTargetForRangeEnemy;
            }

        }
    }
}