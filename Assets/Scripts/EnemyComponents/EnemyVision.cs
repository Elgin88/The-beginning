using System.Collections;
using Assets.Scripts.Tests;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private EnemyVisionPoint _enemyVisionPoint;
        private RaycastHit _raycastHit;
        private Coroutine _vision;
        private float _visionAngle = 160;
        private float _visionRange = 20;
        private float _startVisionRotationY => -1 * _visionAngle / 2;
        private float _currentVisonRotationY;
        private float _finishVisionRotationY => _visionAngle / 2;
        private float _stepOfRotationY => _visionAngle / _rayCount;
        private Ray _ray;
        private int _rayCount = 20;

        internal void StartVision()
        {
            if (_vision == null)
            {
                _vision = StartCoroutine(Vision());
            }
        }

        internal void StopVision()
        {
            if (_vision != null)
            {
                StopCoroutine(_vision);

                _vision = null;
            }
        }

        private void Awake()
        {
            _enemyVisionPoint = GetComponentInChildren<EnemyVisionPoint>();

            StartVision();
        }

        private IEnumerator Vision()
        {
            int currentRayNumber;

            while (true)
            {
                currentRayNumber = 0;

                while (currentRayNumber < _rayCount + 1)
                {
                    currentRayNumber++;
                }

                yield return null;
            }
        }
    }
}