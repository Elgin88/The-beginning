using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.EnemyNamespace
{
    internal class EnemyVision: MonoBehaviour
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyRayPoint _enemyRayPoint;
        [SerializeField] private LayerMask _layersForEnemyVision;
        [SerializeField] private float _visionAngle;
        [SerializeField] private float _visionRange;
        [SerializeField] private float _visionRayCount;

        private Dictionary<Vector3, GameObject> _currentPositionAndTarget;
        private Dictionary<Vector3, GameObject> _positionsAndTargets;
        private float _stepOfRotationY;
        private float _currentDistanceToNearestPositionAndTarget;
        private float _startDistanceToNearestPositionAndTarget = 100;

        internal Dictionary<Vector3, GameObject> CurrentPositionAndTarget => _currentPositionAndTarget;

        internal float DistanceToNearestPositionAndTarget => _currentDistanceToNearestPositionAndTarget;

        private void Awake()
        {
            _currentPositionAndTarget = new Dictionary<Vector3, GameObject>();
            _positionsAndTargets = new Dictionary<Vector3, GameObject>();
            _stepOfRotationY = _visionAngle / _visionRayCount;
            _currentDistanceToNearestPositionAndTarget = _startDistanceToNearestPositionAndTarget;
        }

        private void Start()
        {
            StartCoroutine(Vision());
        }

        private IEnumerator Vision()
        {
            while (true)
            {
                int currentRayNumber = 0;

                _positionsAndTargets = new Dictionary<Vector3, GameObject>();
                _currentDistanceToNearestPositionAndTarget = 1000;

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

            if (raycastHit.collider != null)
            {
                if (raycastHit.collider.gameObject.TryGetComponent(out IDamageable idamageable))
                {
                    if (idamageable.IsPlayerObject & idamageable.IsDead == false)
                    {
                        AddTargetToDictionary(raycastHit.point, raycastHit.collider.gameObject);
                    }
                }
            }

            SetCurrentNearestPOsitionAndTarget();
        }

        private void AddTargetToDictionary(Vector3 raycastHitPoint, UnityEngine.GameObject hitObject)
        {
            _positionsAndTargets.Add(raycastHitPoint, hitObject);
        }

        private void SetCurrentNearestPOsitionAndTarget()
        {
            _currentPositionAndTarget = GetFirstPositionAndTarget();

            if (_positionsAndTargets != null)
            {
                foreach (var item in _positionsAndTargets)
                {
                    if (Vector3.Distance(transform.position, item.Key) < Vector3.Distance(transform.position, GetPositionCurrentTarget()))
                    {
                        _currentPositionAndTarget = new Dictionary<Vector3, UnityEngine.GameObject>() { { item.Key, item.Value } };
                        _currentDistanceToNearestPositionAndTarget = Vector3.Distance(transform.position, item.Key);
                    }
                }
            }
        }

        private Vector3 GetPositionCurrentTarget()
        {
            if (_currentPositionAndTarget != null)
            {
                foreach (var item in _currentPositionAndTarget)
                {
                    return item.Key;
                }
            }

            return Vector3.zero;
        }

        private Dictionary<Vector3, UnityEngine.GameObject> GetFirstPositionAndTarget()
        {
            if (_positionsAndTargets != null)
            {
                if (_positionsAndTargets.Count > 0)
                {
                    foreach (var item in _positionsAndTargets)
                    {
                        Dictionary<Vector3, UnityEngine.GameObject> firstPositionAndTarget = new Dictionary<Vector3, UnityEngine.GameObject> { { item.Key, item.Value } };

                        return firstPositionAndTarget;
                    }
                }
            }

            return null;
        }
    }
}