using System.Collections;
using Assets.Scripts.EnemyNamespace;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal class TransitionMove : Transition
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyRayPoint _enemyRayPoint;
        [SerializeField] private EnemyVision _enemyVision;
        [SerializeField] private StateIdle _stateIdle;
        [SerializeField] private LayerMask _layerMask;

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
            NextState = null;

            while (CheckIsMinDistanceToPlayerObject() == false)
            {
                yield return null;
            }

            NextState = _stateIdle;
        }

        internal bool CheckIsMinDistanceToPlayerObject()
        {
            Debug.DrawRay(_enemyNextTargetFinder.transform.position, _enemyNextTargetFinder.transform.forward * _minDistanteToTarget, Color.red, 0.1f);

            return Physics.Raycast(_enemyNextTargetFinder.transform.position, _enemyNextTargetFinder.transform.forward, _minDistanteToTarget, _layerMask);
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