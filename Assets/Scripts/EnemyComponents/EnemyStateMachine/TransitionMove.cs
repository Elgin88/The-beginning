using System.Collections;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Enemy;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(StateAttack))]
    [RequireComponent(typeof(StateMove))]

    internal class TransitionMove : Transition
    {
        [Inject] private MainBuilding _mainBuilding;

        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private EnemyRayPoint _enemyRayPoint;
        private StateAttack _stateAttack;
        private Coroutine _calculateDistance;
        private float _minDistanceToTarget = 2.0f;

        protected override State NextState { get; set; }

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

        internal override State GetNextState()
        {
            State state = null;
            state = NextState;
            NextState = null;

            return state;
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
            bool isWork = true;

            while (isWork)
            {
                Ray ray = new Ray(_enemyRayPoint.transform.position, transform.forward);

                if (Physics.Raycast(_enemyRayPoint.transform.position, ray.direction, out RaycastHit raysactHit))
                {
                    if (raysactHit.distance <= _minDistanceToTarget & _enemyNextTargetFinder.CurrentTarget.gameObject == raysactHit.collider.gameObject)
                    {
                        NextState = _stateAttack;
                        isWork = false;
                    }
                }

                yield return null;
            }
        }
    }
}