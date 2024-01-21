using Assets.Scripts.Enemy;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    internal class TransitionMove : Transition
    {
        private EnemyRayPoint _enemyRayPoint;
        private float _minDistanceToTarget = 2;

        private Coroutine _calculateDistance;

        private void Awake()
        {
            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();

            _calculateDistance = StartCoroutine(CalculateDistance());
        }

        private IEnumerator CalculateDistance()
        {

            yield return null;
        }
    }
}