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
        [SerializeField] private float _maxAngle;
        [SerializeField] private float _range;
        [SerializeField] private float _rayCount;

        private Dictionary<Vector3, GameObject> _currentPositionAndTarget;
        private Dictionary<Vector3, GameObject> _positionsAndTargets;
        private Dictionary<Vector3, GameObject> _firstPositionAndTarget;
        private Coroutine _vision;
        private float _stepOfRotationY;
        private float _currentDistanceToNearestPositionAndTarget;
        private float _startDistanceToNearestPositionAndTarget = 100;

        internal Dictionary<Vector3, GameObject> CurrentPositionAndTarget => _currentPositionAndTarget;

        internal float DistanceToNearestPositionAndTarget => _currentDistanceToNearestPositionAndTarget;

        private void Awake()
        {
            _currentPositionAndTarget = new Dictionary<Vector3, GameObject>();
            _positionsAndTargets = new Dictionary<Vector3, GameObject>();
            _firstPositionAndTarget = new Dictionary<Vector3, GameObject>();

            _stepOfRotationY = _maxAngle / _rayCount;
            _currentDistanceToNearestPositionAndTarget = _startDistanceToNearestPositionAndTarget;
        }

        private void Start()
        {
            StartVision();
        }

        private void StartVision()
        {
            _vision = StartCoroutine(Vision());
        }

        private void StopVision()
        {
            StopCoroutine(_vision);
        }

        private IEnumerator Vision()
        {
            while (true)
            {
                int currentRayNumber = 0;

                _positionsAndTargets.Clear();
                _currentDistanceToNearestPositionAndTarget = 1000;

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
            _enemyRayPoint.transform.localRotation = Quaternion.Euler(_enemyRayPoint.transform.localRotation.x, - 90 + (180 - _maxAngle)/2 + _stepOfRotationY * currentRayNumber, _enemyRayPoint.transform.localRotation.z);
        }

        private void SetDataRaycastHit()
        {
            Physics.Raycast(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward, out RaycastHit raycastHit, _range, _layersForEnemyVision);

            Debug.DrawRay(_enemyRayPoint.transform.position, _enemyRayPoint.transform.forward * _range, Color.yellow, 0.1f);

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

            SetCurrentNearestPositionAndTarget();
        }

        private void AddTargetToDictionary(Vector3 raycastHitPoint, GameObject hitObject)
        {
            _positionsAndTargets.Add(raycastHitPoint, hitObject);
        }

        private void SetCurrentNearestPositionAndTarget()
        {
            _currentPositionAndTarget = GetFirstPositionAndTarget();

            if (_positionsAndTargets != null)
            {
                foreach (var item in _positionsAndTargets)
                {
                    if (Vector3.Distance(transform.position, item.Key) < Vector3.Distance(transform.position, GetPositionCurrentTarget()))
                    {
                        _currentPositionAndTarget.Clear();
                        _currentPositionAndTarget.Add(item.Key, item.Value);
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

        private Dictionary<Vector3, GameObject> GetFirstPositionAndTarget()
        {
            _firstPositionAndTarget.Clear();

            if (_positionsAndTargets != null)
            {
                if (_positionsAndTargets.Count > 0)
                {
                    foreach (var item in _positionsAndTargets)
                    {
                        _firstPositionAndTarget.Add(item.Key, item.Value);

                        return _firstPositionAndTarget;
                    }
                }
            }

            return null;
        }
    }
}