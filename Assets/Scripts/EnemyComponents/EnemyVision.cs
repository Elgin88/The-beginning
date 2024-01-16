using System.Collections;
using Assets.Scripts.Tests;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyVision: MonoBehaviour
    {
        private EnemyVisionPoint _enemyVisionPoint;
        private RaycastHit _raycastHit;
        private Coroutine _changeVisionPointRotation = null;
        private float _leftViewVisionPoint = -160;
        private float _righttViewVisionPoint = 160;
        private bool _isRightTurnVisionPoint = true;
        private Ray _ray;

        internal void StartChangeVisionPointRotation()
        {
            if (_changeVisionPointRotation == null)
            {
                _changeVisionPointRotation = StartCoroutine(ChangeVisionPointRotation());
            }
        }

        internal void StopChangeVisionPointRotation()
        {
            if (_changeVisionPointRotation != null)
            {
                StopCoroutine(_changeVisionPointRotation);

                _changeVisionPointRotation = null;
            }
        }

        private void Awake()
        {
            _enemyVisionPoint = GetComponentInChildren<EnemyVisionPoint>();

            _ray = new Ray(_enemyVisionPoint.transform.position, _enemyVisionPoint.transform.forward);

            StartChangeVisionPointRotation();
        }

        private IEnumerator ChangeVisionPointRotation()
        {
            while (true)
            {
                Debug.DrawRay(_enemyVisionPoint.transform.position, _enemyVisionPoint.transform.forward * 20, Color.red);

                yield return null;
            }
        }
    }
}