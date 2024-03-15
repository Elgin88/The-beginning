using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        [SerializeField] private EnemyRayPoint _enemyRayPoint;
        [SerializeField] private LayerMask _layersForEnemyVision;
        [SerializeField] private float _visionAngle;
        [SerializeField] private float _visionRange;
        [SerializeField] private float _visionRayCount;

        private List<GameObject> _targets;
        private float _stepOfRotationY => _visionAngle / _visionRayCount;

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

                while (currentRayNumber <= _visionRayCount)
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
            Physics.Raycast(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward, out RaycastHit raycastHit, _visionRange, _layersForEnemyVision);

            //Debug.DrawRay(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward * _visionRange, Color.yellow);

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