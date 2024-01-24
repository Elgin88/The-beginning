using System.Collections;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.PlayerComponents;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    internal class EnemyNextTargetFinder: MonoBehaviour
    {
        [Inject] private MainBuilding _mainBuilding;

        private Coroutine _findTarget;
        private EnemyVision _enemyVision;
        private GameObject _nextTarget;

        public GameObject NextTarget => _nextTarget;

        private void Awake()
        {
            _enemyVision = GetComponent<EnemyVision>();

            _nextTarget = _mainBuilding.gameObject;

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
            while (true)
            {
                bool isPlayer = false;

                foreach (GameObject target in _enemyVision.Targets)
                {
                    if (_enemyVision.Targets.Count == 0)
                    {
                        _nextTarget = _mainBuilding.gameObject;
                    }
                    else if (target.TryGetComponent<Player>(out Player player))
                    {
                        _nextTarget = target;
                        isPlayer = true;
                    }
                    else if (target.TryGetComponent<Building>(out Building building) & isPlayer == false)
                    {
                        if (true)
                        {

                        }

                        _nextTarget = target;
                    }
                    else
                    {
                        _nextTarget = _mainBuilding.gameObject;
                    }
                }

                yield return null;
            }
        }
    }
}