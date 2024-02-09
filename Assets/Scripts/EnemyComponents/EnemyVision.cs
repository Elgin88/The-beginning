using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private EnemyRayPoint _enemyRayPoint;
        private List<GameObject> _targets;
        private GameObject _currentTarget;
        private float _visionAngle = 160;
        private float _visionRange = 10;
        private float _stepOfRotationY => _visionAngle / _rayCount;
        private int _rayCount = 100;

        internal GameObject GetCloseTarget()
        {
            if (_targets.Count != 0)
            {
                _currentTarget = _targets[0];

                foreach (GameObject target in _targets)
                {
                    if (Vector3.Distance(transform.position, target.transform.position) < Vector3.Distance(transform.position, _currentTarget.transform.position))
                    {
                        _currentTarget = target;
                    }
                }
            }

            return _currentTarget;
        }

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
        }

        private void SetDataRaycastHit(Ray ray)
        {
            RaycastHit raycastHit;

            Physics.Raycast(ray, out raycastHit);

            if (raycastHit.collider != null)
            {
                if (raycastHit.collider.gameObject.TryGetComponent<IDamageable>(out IDamageable idamageable) & raycastHit.distance <= _visionRange)
                {
                    if (idamageable.IsPlayerObject & idamageable.IsDead == false)
                    {
                        AddTargetInList(raycastHit.collider.gameObject);
                    }
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