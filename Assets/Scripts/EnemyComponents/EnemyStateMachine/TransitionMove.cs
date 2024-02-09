using System;
using System.Collections;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Enemy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(StateAttack))]
    [RequireComponent(typeof(StateMove))]

    internal class TransitionMove : Transition
    {
        [Inject] private MainBuilding _mainBuilding;

        private float _minDistanceToTarget = 2.0f;

        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private StateMove _stateMove;
        private EnemyRayPoint _enemyRayPoint;
        private StateAttack _stateAttack;
        private Coroutine _calculateDistance;

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
            _stateMove = GetComponent<StateMove>();
            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();

            StartCallculateDistance();
        }

        private IEnumerator CalculateDistance()
        {
            IsNeedNextState = false;

            yield return null;

            while (_mainBuilding != null)
            {
                Ray ray = new Ray(_enemyRayPoint.transform.position, transform.forward);

                if (Physics.Raycast(_enemyRayPoint.transform.position, ray.direction, out RaycastHit raysactHit))
                {
                    if (raysactHit.distance < _minDistanceToTarget & _enemyNextTargetFinder.CurrentTarget.gameObject == raysactHit.collider.gameObject)
                    {
                        IsNeedNextState = true;
                        NextState = _stateAttack;
                        _stateMove.ResetPath();
                    }
                }

                yield return null;
            }
        }
    }
}