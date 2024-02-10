using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private EnemyRayPoint _enemyRayPoint;
        private List<GameObject> _targets;
        private float _visionAngle = 160;
        private float _visionRange = 10;
        private float _stepOfRotationY => _visionAngle / _rayCount;
        private int _rayCount = 100;

        public List<GameObject> Targets => _targets;

        private void Awake()
        {
            _enemyRayPoint = GetComponentInChildren<EnemyRayPoint>();

            _targets = new List<GameObject>();

            StartCoroutine(Vision());
        }

        private IEnumerator Vision()
        {
            while (true)
            {
                int currentRayNumber = 0;

                _targets = new List<GameObject>();

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

            Debug.DrawRay(_enemyRayPoint.transform.position, ray.direction * _visionRange, Color.yellow, 0.1f);
        }

        private void SetDataRaycastHit(Ray ray)
        {
            RaycastHit raycastHit;

            Physics.Raycast(ray, out raycastHit);

            if (raycastHit.collider != null)
            {
                if (raycastHit.collider.gameObject.TryGetComponent<Terrain>(out Terrain terrain) == false & raycastHit.distance <= _visionRange)
                {
                    AddTargetInList(raycastHit.collider.gameObject);
                }
            }
        }

        private void AddTargetInList(GameObject gameObject)
        {
            if (CheckTargetsForRepeat(gameObject) == false)
            {
                _targets.Add(gameObject);
            }
        }

        private bool CheckTargetsForRepeat(GameObject gameObject)
        {
            bool isRepeat = false;

            foreach (GameObject target in _targets)
            {
                if (target == gameObject)
                {
                    isRepeat = true;
                }
            }

            return isRepeat;
        }
    }
}