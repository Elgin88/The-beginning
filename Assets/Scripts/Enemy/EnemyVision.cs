using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private List<GameObject> _targets;
        private EnemyRayPoint _enemyRayPoint;
        private float _visionAngle = 170;
        private float _visionRange = 20;
        private float _stepOfRotationY => _visionAngle / _rayCount;
        private int _rayCount = 100;

        internal List<GameObject> GetTargets()
        {
            List<GameObject> targets;

            targets = _targets;

            return targets;
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
                    SetDataRaycastHit();

                    currentRayNumber++;
                }

                yield return null;
            }
        }

        private void SetEnemyRayPointRotation(int currentRayNumber)
        {
            _enemyRayPoint.transform.localRotation = Quaternion.Euler(_enemyRayPoint.transform.localRotation.x, - 90 + (180 - _visionAngle)/2 + _stepOfRotationY * currentRayNumber, _enemyRayPoint.transform.localRotation.z);
        }

        private void SetDataRaycastHit()
        {
            Physics.Raycast(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward, out RaycastHit raycastHit, _visionRange);

            //Debug.DrawRay(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward * _visionRange, Color.yellow, 0.1f);

            if (raycastHit.collider != null)
            {
                if (raycastHit.collider.gameObject.TryGetComponent(out IDamageable idamageable))
                {
                    if (idamageable.IsPlayerObject & idamageable.IsDead == false)
                    {
                        AddTargetToList(raycastHit.collider.gameObject);
                    }
                }
            }
        }

        private void AddTargetToList(GameObject gameObject)
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