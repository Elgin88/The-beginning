using System.Collections;
using UnityEngine;
using Assets.Scripts.Enemy;

namespace Assets.Scripts.UnitStateMachine
{
    internal class TransitionMove : Transition
    {
        private EnemyRayPoint _enemyRayPoint;
        private Ray _ray;
        private float _minDistanceToTarget = 2;

        private Coroutine _calculateDistance;

        private void Awake()
        {
            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();

            _calculateDistance = StartCoroutine(CalculateDistance());

            _ray = new Ray(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward);
        }

        private IEnumerator CalculateDistance()
        {
            Debug.Log("Дописать здесь");


            yield return null;
        }
    }
}