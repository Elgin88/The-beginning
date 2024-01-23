using Assets.Scripts.ConStants;
using Assets.Scripts.Enemy;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.UnitStateMachine;
using Assets.Scripts.Tests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.PlayerComponents;
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
                if (_enemyVision.Target == null)
                {
                    _nextTarget = _mainBuilding.gameObject;
                }
                else if (_enemyVision.Target.gameObject.TryGetComponent<Player>(out Player player))
                {
                    _nextTarget = _enemyVision.Target;
                    StopFindTarget();
                }
                else if (_enemyVision.Target.gameObject.TryGetComponent<Building>(out Building building))
                {
                    _nextTarget = _enemyVision.Target;
                    StopFindTarget();
                }
                else
                {
                    _nextTarget = _mainBuilding.gameObject;
                }

                yield return null;
            }
        }
    }
}