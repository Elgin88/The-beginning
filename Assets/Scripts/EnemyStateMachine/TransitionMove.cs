using System;
using System.Collections;
using Assets.Scripts.Enemy;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(StateAttack))]

    internal class TransitionMove : Transition
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private EnemyRayPoint _enemyRayPoint;
        private StateAttack _stateAttack;
        private RaycastHit _raycastHit;
        private Coroutine _calculateDistance;
        private State _nextState;
        private Ray _ray;
        private float _currentDistanceToTarget;
        private float _minDistanceToTarget = 2;

        public override State NextState { get ; set ; }

        public override bool IsNeedNextState { get ; set; }

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
            _stateAttack = GetComponent<StateAttack>();
            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();

            StartCallculateDistance();
        }

        private IEnumerator CalculateDistance()
        {
            while (true)
            {
                CalculateDistanceToTarget();

                yield return null;
            }

            SetNextState(_stateAttack);
            StopCallculateDistance();
        }

        private void SetNextState(State state)
        {
            NextState = state;
            IsNeedNextState = true;
        }

        private void CalculateDistanceToTarget()
        {

        }
    }
}