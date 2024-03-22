using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyNextTargetFinder : MonoBehaviour
    {
        [SerializeField] private EnemyVision _enemyVision;

        private Dictionary<Vector3, UnityEngine.GameObject> _currentPositionAndTarget;
        private MainBuilding _mainBuilding; 
        private GameObject _startTarget;
        private GameObject _currentTarget;
        private Coroutine _setNextTarget;
        private Vector3 _currentTargetPosition;

        internal Vector3 CurrentTargetPosition => _currentTargetPosition;

        internal GameObject CurrentTarget => _currentTarget;

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
                _currentTargetPosition = Vector3.zero;
                _currentTarget = null;

                if (_enemyVision.CurrentPositionAndTarget != null)
                {
                    _currentPositionAndTarget = _enemyVision.CurrentPositionAndTarget;
                }

                if (_currentPositionAndTarget != null)
                {
                    foreach (var item in _currentPositionAndTarget)
                    {
                        _currentTargetPosition = item.Key;
                        _currentTarget = item.Value;
                    }
                }

                if (_currentTarget == null & _startTarget != null)
                {
                    _currentTarget = _startTarget;
                    _currentTargetPosition = _startTarget.transform.position;
                }

                yield return null;
            }
        }

        private void OnEnable()
        {
            if (_mainBuilding != null)
            {
                _startTarget = _mainBuilding.gameObject;
            }

            if (_startTarget != null)
            {
                _currentTarget = _startTarget;
                _currentTargetPosition = _startTarget.transform.position;
            }

            StartSetNextTarget();
        }

        internal void InitMainBuilding(MainBuilding mainBuilding)
        {
            _mainBuilding = mainBuilding;
        }
    }
}