using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(EnemyVision))]

    internal class EnemyNextTargetFinder : MonoBehaviour
    {
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