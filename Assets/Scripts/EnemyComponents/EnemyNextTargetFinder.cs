using System.Collections;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.PlayerComponents;
using UnityEngine;
using Zenject;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.Enemy
{
    internal class EnemyNextTargetFinder: MonoBehaviour
    {
        [Inject] private MainBuilding _mainBuilding;

        private Coroutine _findTarget;
        private EnemyVision _enemyVision;
        private GameObject _currentTarget;

        public GameObject NextTarget => _currentTarget;

        private void Awake()
        {
            _enemyVision = GetComponent<EnemyVision>();

            _currentTarget = _mainBuilding.gameObject;

            StartFindTarget();
        }

        public void StartFindTarget()
        {
            _findTarget = StartCoroutine(FindTarget());
        }

        public void StopFindTarget()
        {
            StopCoroutine(_findTarget);
        }

        private IEnumerator FindTarget()
        {
            while (_currentTarget == _mainBuilding.gameObject)
            {
                foreach (GameObject nextTarget in _enemyVision.Targets)
                {
                    nextTarget.TryGetComponent<IDamageable>(out IDamageable idamageable);

                    if (_enemyVision.Targets.Count == 0)
                    {
                        _currentTarget = _mainBuilding.gameObject;
                    }
                    else if (idamageable.IsPlayerObject)
                    {
                        if (Vector3.Distance(transform.position, _currentTarget.transform.position) > Vector3.Distance(transform.position, nextTarget.transform.position))
                        {
                            _currentTarget = nextTarget;
                        }
                    }
                    else
                    {
                        _currentTarget = _mainBuilding.gameObject;
                    }
                }

                yield return null;
            }

            StopFindTarget();
        }
    }
}