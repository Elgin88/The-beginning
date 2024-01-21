using System;
using System.Collections;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.Tests;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private GameObject _target;
        private EnemyRayPoint _enemyRayPoint;
        private float _visionAngle = 180;
        private float _visionRange = 20;
        private float _stepOfRotation => _visionAngle / _rayCount;
        private int _rayCount = 10;

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
                int numberOfRay = 1;

                SetStartRaypointRotation();

                while (numberOfRay <= _rayCount)
                {
                    //SetCurrentRayPointRotation(numberOfRay);
                    CreateAndShowRay(out Ray ray);
                    GetDataRaycastHit(ray);
                    numberOfRay++;
                }
                
                yield return null;
            }
        }

        private void SetStartRaypointRotation()
        {
            _enemyRayPoint.transform.localRotation = Quaternion.Euler(
                transform.localRotation.x,
                transform.localRotation.y,
                transform.localRotation.z);
        }

        private void SetCurrentRayPointRotation(int numberOfRay)
        {
            _enemyRayPoint.transform.rotation = Quaternion.Euler(
                transform.rotation.x,
                transform.rotation.y + _stepOfRotation * numberOfRay,
                transform.rotation.z);
        }











        private void CreateAndShowRay(out Ray ray)
        {
            ray = new Ray(_enemyRayPoint.transform.position, transform.forward) ;
            Debug.DrawRay(_enemyRayPoint.transform.position, transform.forward * _visionRange, Color.red, 0.3f);
        }

        private void GetDataRaycastHit(Ray ray)
        {
            Physics.Raycast(ray, out RaycastHit raycastHit);

            if (raycastHit.collider != null & raycastHit.distance <= _visionRange)
            {
                if (raycastHit.collider.GetComponent<MainBuilding>() != null || raycastHit.collider.GetComponent<Player>() != null)
                {
                    _target = raycastHit.collider.gameObject;

                    Debug.Log(_target.name);
                }
            }
            else
            {
                _target = null;
            }
        }
    }
}