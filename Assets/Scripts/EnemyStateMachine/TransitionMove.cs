using System;
using System.Collections;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Enemy;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(StateAttack))]

    internal class TransitionMove : Transition
    {
        [Inject] private MainBuilding _mainBuilding;

        private float _minDistanceToTarget = 2.0f;

        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private EnemyRayPoint _enemyRayPoint;
        private StateAttack _stateAttack;
        private Coroutine _calculateDistance;
        private RaycastHit _raysactHit;
        private Ray _ray;

        protected override State NextState { get; set; }

        protected override bool IsNeedNextState { get; set; }

        internal override bool GetIsNeedNextState()
        {
            return IsNeedNextState;
        }

        internal override State GetNextState()
        {
            return NextState;
        }

        internal void StartCallculateDistance()
        {
            if (_calculateDistance == null)
            {
                _calculateDistance = StartCoroutine(CalculateDistance());
            }
        }

        internal void StopCallculateDistance()
        {
            if (_calculateDistance != null)
            {
                StopCoroutine(_calculateDistance);
                _calculateDistance = null;
            }
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
            IsNeedNextState = false;

            yield return null;

            while (_mainBuilding != null)
            {
                _ray = new Ray(_enemyRayPoint.transform.position, transform.forward);

                Debug.DrawRay(_enemyRayPoint.transform.position, _ray.direction * 100, Color.red, 0.1f);


                if (Physics.Raycast(_enemyRayPoint.transform.position, _ray.direction, out _raysactHit))
                {
                    if (_raysactHit.distance < _minDistanceToTarget & _enemyNextTargetFinder.CurrentTarget.gameObject == _raysactHit.collider.gameObject)
                    {
                        IsNeedNextState = true;
                        NextState = _stateAttack;
                    }
                }

                yield return null;
            }
        }
    }
}