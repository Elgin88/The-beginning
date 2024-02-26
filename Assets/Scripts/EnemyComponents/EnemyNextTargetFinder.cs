using Assets.Scripts.BuildingSystem.Buildings;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(EnemyVision))]

    internal class EnemyNextTargetFinder : MonoBehaviour
    {
        private MainBuilding _mainBulding;
        private EnemyVision _enemyVision;
        private GameObject _currentTarget;
        private Coroutine _findNextTarget;

        internal GameObject CurrentTarget => _currentTarget;

        internal void StartFindNextTarget()
        {
            if (_findNextTarget == null)
            {
                _findNextTarget = StartCoroutine(FindNextTarget());
            }
        }

        internal void StopFindNextTarget()
        {
            if (_findNextTarget != null)
            {
                StopCoroutine(_findNextTarget);
                _findNextTarget = null;
            }
        }

        internal GameObject GetCloseTarget()
        {
            _currentTarget = null;

            SetMainBuildingAsTarget();
            SelectNearbyObjectAsTarget();

            return _currentTarget;
        }

        private void SetMainBuildingAsTarget()
        {
            if (_mainBulding != null)
            {
                _currentTarget = _mainBulding.gameObject;
            }
        }

        private void SelectNearbyObjectAsTarget()
        {
            if (_enemyVision.GetTargets().Count > 0)
            {
                _currentTarget = _enemyVision.GetTargets()[0];
            }

            foreach (GameObject target in _enemyVision.GetTargets())
            {
                if (_enemyVision.GetTargets().Count > 1)
                {
                    if (Vector3.Distance(transform.position, target.transform.position) < Vector3.Distance(transform.position, _currentTarget.transform.position))
                    {
                        _currentTarget = target;
                    }
                }
            }
        }

        private void Awake()
        {
            _mainBulding = FindAnyObjectByType<MainBuilding>();

            _enemyVision = GetComponent<EnemyVision>();

            StartFindNextTarget();
        }

        private IEnumerator FindNextTarget()
        {
            while (true)
            {
                _currentTarget = GetCloseTarget();

                yield return null;
            }
        }
    }
}