using System.Collections;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    internal class EnemyNextTargetFinder : MonoBehaviour
    {
        private GameObject _currentTarget;
        private Coroutine _findNextTarget;
        private EnemyVision _enemyVision;

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

            StartFindNextTarget();
        }

        private IEnumerator FindNextTarget()
        {
            while (true)
            {
                _currentTarget = _enemyVision.GetCloseTarget();

                yield return null;
            }
        }
    }
}