using System.Collections;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.BuildingSystem.Buildings;

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

        private void Awake()
        {
            _enemyVision = GetComponent<EnemyVision>();
            _mainBulding = FindAnyObjectByType<MainBuilding>();

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

        internal GameObject GetCloseTarget()
        {
            _currentTarget = _enemyVision.GetTargets()[0];

            foreach (GameObject target in _enemyVision.GetTargets())
            {
                if (target != null)
                {
                    if (target.gameObject.TryGetComponent(out IDamageable idamageable) & idamageable.IsDead == false)
                    {
                        if (target != null & _currentTarget != null)
                        {
                            if (Vector3.Distance(transform.position, target.transform.position) < Vector3.Distance(transform.position, _currentTarget.transform.position))
                            {
                                _currentTarget = target;
                            }
                        }
                        else if (_mainBulding != null)
                        {
                            _currentTarget = _mainBulding.gameObject;
                        }
                    }
                }
            }

            return _currentTarget;
        }
    }
}