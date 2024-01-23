using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.Tests;
using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private EnemyRayPoint _enemyRayPoint;
        private GameObject _target;
        private float _visionAngle = 160;
        private float _visionRange = 30;
        private float _stepOfRotationY => _visionAngle / _rayCount;
        private int _rayCount = 100;

        public GameObject Target => _target;

        private void Awake()
        {
            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();

            StartCoroutine(Vision());
        }

        private IEnumerator Vision()
        {
            while (true)
            {
                int currentRayNumber = 0;

                while (currentRayNumber <= _rayCount)
                {
                    SetEnemyRayPointRotation(currentRayNumber);
                    CreateRay(out Ray ray);
                    SetDataRaycastHit(ray);

                    currentRayNumber++;
                }

                yield return null;
            }
        }

        private void SetEnemyRayPointRotation(int currentRayNumber)
        {
            _enemyRayPoint.transform.localRotation = Quaternion.Euler(_enemyRayPoint.transform.localRotation.x, - 90 + (180 - _visionAngle)/2 + _stepOfRotationY * currentRayNumber, _enemyRayPoint.transform.localRotation.z);
        }

        private void CreateRay(out Ray ray)
        {
            ray = new Ray(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward);

            Debug.DrawRay(_enemyRayPoint.transform.position, ray.direction * _visionRange, Color.red, 0.01f);
        }

        private void SetDataRaycastHit(Ray ray)
        {
            RaycastHit raycastHit;

            Physics.Raycast(ray, out raycastHit);

            if (raycastHit.collider != null & raycastHit.distance <= _visionRange)
            {
                if (raycastHit.collider.gameObject.TryGetComponent<Terrain>(out Terrain terrain) == false)
                {
                    _target = raycastHit.collider.gameObject;
                }
            }
        }
    }
}