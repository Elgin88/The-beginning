using System;
using System.Collections;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.Tests;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private EnemyRayPoint _enemyRayPoint;
        private Coroutine _vision;
        private float _visionAngle = 160;
        private float _visionRange = 20;
        private float _startVisionEulerRotationY => 90 - _visionAngle / 2;
        private float _finishVisionRotationY;
        private float _stepOfRotationY => _visionAngle / _rayCount;
        private RaycastHit _raycastHit;
        private Ray _ray;
        private int _rayCount = 40;

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
            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();

            StartVision();
        }

        private IEnumerator Vision()
        {
            int currentRayNumber;
            Ray ray;


            while (true)
            {
                currentRayNumber = 0;

                SetStartRayPointPositon();

                while (currentRayNumber < _rayCount + 1)
                {
                    SetDataRaycastHit();                         // Не сделано

                    _ray = new Ray(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward);
                    Debug.DrawRay(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward * 20, Color.red, 0.5f);

                    SetNextRayPointPosition();                   // Не сделано

                    currentRayNumber++;
                }



                yield return null;
            }
        }

        private void SetStartRayPointPositon()
        {
            _enemyRayPoint.transform.rotation = Quaternion.Euler(_enemyRayPoint.transform.rotation.eulerAngles.x, _startVisionEulerRotationY, _enemyRayPoint.transform.rotation.eulerAngles.z);
        }

        private void SetNextRayPointPosition()
        {
            _enemyRayPoint.transform.rotation = Quaternion.Euler(_enemyRayPoint.transform.rotation.eulerAngles.x, _enemyRayPoint.transform.rotation.eulerAngles.y + _stepOfRotationY, _enemyRayPoint.transform.rotation.eulerAngles.z);
        }

        private void SetDataRaycastHit()
        {
            Physics.Raycast(_ray, out _raycastHit);

            if (_raycastHit.collider != null)
            {
                if (_raycastHit.collider.GetComponent<MainBuilding>() != null || _raycastHit.collider.GetComponent<Player>() != null)
                {
                    if (_raycastHit.distance <= _visionRange)
                    {
                        Debug.Log(_raycastHit.collider.gameObject.name);
                    }
                }
            }
        }
    }
}