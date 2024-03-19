using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyNextTargetFinder : MonoBehaviour
    {
        [SerializeField] private EnemyVision _enemyVision;

        private Dictionary<Vector3, GameObject> _currentPositionAndTarget;
        private MainBuilding _mainBulding;
        private GameObject _startTarget;
        private GameObject _currentTarget;
        private Coroutine _setNextTarget;
        private Vector3 _currentTargetPosition;

        internal GameObject StartTarget => _startTarget;

        internal GameObject CurrentTarget => _currentTarget;

        internal Vector3 CurrentTargetPosition => _currentTargetPosition;

        internal void StartSetNextTarget()
        {
            _setNextTarget = StartCoroutine(SetNextTarget());
        }

        internal void StopSetNextTarget()
        {
            StopCoroutine(_setNextTarget);
        }

        private IEnumerator SetNextTarget()
        {
            while (true)
            {
                _currentPositionAndTarget = _enemyVision.CurrentPositionAndTarget;

                if (_currentPositionAndTarget != null)
                {
                    foreach (var item in _currentPositionAndTarget)
                    {
                        _currentTargetPosition = item.Key;
                        _currentTarget = item.Value;
                    }
                }

                yield return null;
            }
        }

        private void Awake()
        {
            _mainBulding = FindAnyObjectByType<MainBuilding>();

            _startTarget = _mainBulding.gameObject;
            _currentTarget = _startTarget;
        }

        private void Start()
        {
            StartSetNextTarget();
        }
    }
}