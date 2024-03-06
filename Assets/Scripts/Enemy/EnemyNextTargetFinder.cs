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

        private IEnumerator FindNextTarget()
        {
            while (true)
            {
                _currentTarget = null;

                if (_mainBulding != null)
                {
                    _currentTarget = _mainBulding.gameObject;
                }

                if (_enemyVision.GetTargets().Count > 0)
                {
                    _currentTarget = _enemyVision.GetTargets()[0];
                }

                foreach (GameObject target in _enemyVision.GetTargets())
                {
                    if (_enemyVision.GetTargets().Count > 1 & _currentTarget != null & target != null)
                    {
                        if (Vector3.Distance(transform.position, target.transform.position) < Vector3.Distance(transform.position, _currentTarget.transform.position))
                        {
                            _currentTarget = target;
                        }
                    }
                }

                yield return null;
            }
        }

        private void Awake()
        {
            _mainBulding = FindAnyObjectByType<MainBuilding>();
            _enemyVision = GetComponent<EnemyVision>();
        }

        private void Start()
        {
            StartFindNextTarget();
        }
    }
}