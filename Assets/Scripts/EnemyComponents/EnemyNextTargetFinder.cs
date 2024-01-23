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

        private EnemyVision _enemyVision;
        private GameObject _nextTarget;

        public GameObject NextTarget => _nextTarget;

        private void Awake()
        {
            _enemyVision = GetComponent<EnemyVision>();

            _nextTarget = _mainBuilding.gameObject;

            StartCoroutine(FindTarget());
        }

        private IEnumerator FindTarget()
        {
            while (true)
            {
                if (_enemyVision.Target == null)
                {
                    Debug.Log("1");
                    _nextTarget = _mainBuilding.gameObject;
                }
                else if(_enemyVision.Target.gameObject.TryGetComponent<Player>(out Player player))
                {
                    _nextTarget = _enemyVision.Target;
                }
                else if (_enemyVision.Target.gameObject.TryGetComponent<Building>(out Building building))
                {
                    _nextTarget = _enemyVision.Target;
                }
                else
                {
                    _nextTarget = _mainBuilding.gameObject;
                }

                Debug.Log(_nextTarget.name);

                yield return null;
            }
        }
    }
}