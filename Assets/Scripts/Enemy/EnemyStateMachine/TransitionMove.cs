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
        private bool _isMinDistance;

        protected override Coroutine CheckTransition { get; set; }

        protected override State NextState { get; set; }

        internal override State GetNextState()
        {
            return NextState;
        }

        internal override IEnumerator CheckTransitionIE()
        {
            _isMinDistance = false;
            NextState = null;

            while (_isMinDistance == false)
            {
                _isMinDistance = CheckIsMinDistanceToPlayerObject();

                yield return null;
            }

            NextState = _stateIdle;
        }

        internal bool CheckIsMinDistanceToPlayerObject()
        {
            bool isMinDistance = false;

            if (Physics.Raycast(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward, _minDistanteToTarget, _layerMask))
            {
                isMinDistance = true;
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