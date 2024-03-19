using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private EnemyRayPoint _enemyRayPoint;
        [SerializeField] private LayerMask _layersForEnemyVision;
        [SerializeField] private float _visionAngle;
        [SerializeField] private float _visionRange;
        [SerializeField] private float _visionRayCount;

        private Dictionary<Vector3, GameObject> _currentNearestPositionAndTarget;
        private Dictionary<Vector3,GameObject> _currentPositionsAndTargets;
        private float _stepOfRotationY;
        private float _currentDistanceToNearestPositionAndTarget;

        internal Dictionary<Vector3,GameObject> CurrentPositionAndTarget => _currentNearestPositionAndTarget;

        internal float DistanceToNearestPositionAndTarget => _currentDistanceToNearestPositionAndTarget;

        private void Awake()
        {
            _currentNearestPositionAndTarget = new Dictionary<Vector3, GameObject>();
            _currentPositionsAndTargets = new Dictionary<Vector3, GameObject>();
            _stepOfRotationY = _visionAngle / _visionRayCount;
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

                _currentPositionsAndTargets = new Dictionary<Vector3, GameObject>();

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
            Debug.DrawRay(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward * _visionRange, Color.yellow);

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

        private void AddTargetToDictionary(Vector3 raycastHitPoint, GameObject hitObject)
        {
            _currentPositionsAndTargets.Add(raycastHitPoint, hitObject);
        }

        private void SetCurrentNearestPOsitionAndTarget()
        {
            _currentNearestPositionAndTarget = GetFirstPositionAndTarget();
            _currentDistanceToNearestPositionAndTarget = Vector3.Distance(transform.position, _enemyNextTargetFinder.StartTarget.transform.position);

            foreach (var item in _currentPositionsAndTargets)
            {
                if (Vector3.Distance(transform.position, item.Key) < Vector3.Distance(transform.position, GetPositionCurrentTarget()))
                {
                    _currentNearestPositionAndTarget = new Dictionary<Vector3, GameObject>() { { item.Key, item.Value } };
                    _currentDistanceToNearestPositionAndTarget = Vector3.Distance(transform.position, item.Key);
                }
            }
        }

        private Vector3 GetPositionCurrentTarget()
        {
            if (_currentNearestPositionAndTarget != null)
            {
                foreach (var item in _currentNearestPositionAndTarget)
                {
                    return item.Key;
                }
            }

            return _enemyNextTargetFinder.StartTarget.transform.position;
        }

        private Dictionary<Vector3, GameObject> GetFirstPositionAndTarget()
        {
            if (_currentPositionsAndTargets.Count > 0)
            {
                foreach (var item in _currentPositionsAndTargets)
                {
                    Dictionary<Vector3, GameObject> firstPositionAndTarget = new Dictionary<Vector3, GameObject> { { item.Key, item.Value } };

                    return firstPositionAndTarget;
                }
            }

            return null;
        }
    }
}