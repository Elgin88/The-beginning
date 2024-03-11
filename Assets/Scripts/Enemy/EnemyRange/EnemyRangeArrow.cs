using System.Collections;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.Enemy
{
    public class EnemyRangeArrow : MonoBehaviour
    {
        [SerializeField] private EnemyRangeWoodArcher _enemyRangeWoodArcher;

        private Coroutine _fly;
        private Transform _currentTarget;
        private Transform _startTarget;
        private Vector3 _currentTargetPosition;
        private Vector3 _startTrajectoryPosition;
        private Vector3 _middleTrajectoryPosition;
        private float _speedOfMove = 20;
        private float _speedOfRotation = 20;
        private float _hight = 0.5f;
        private bool _isMoveUp;

        internal void StartFly(Transform target)
        {
            _startTarget = target;
            _startTrajectoryPosition = transform.position;
            _isMoveUp = true;

            _fly = StartCoroutine(Fly());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currentTarget != null)
            {
                if (other.gameObject == _currentTarget.gameObject)
                {
                    _currentTarget.GetComponent<IDamageable>().TakeDamage(_enemyRangeWoodArcher.GetComponent<IEnemy>().Damage);
                    StopFly();
                    gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator Fly()
        {
            _currentTarget = _startTarget;

            while (true)
            {
                if (_currentTarget == null || _startTrajectoryPosition == null || _currentTarget.position == null || _currentTarget != _startTarget)
                {
                    StopFly();
                    gameObject.SetActive(false);
                    yield break;
                }

                CalculateDistane(_startTrajectoryPosition, _currentTarget.position);
                SetMiddleTrajectoryPosition();
                CheckIsMoveUp();
                CalculateTargetPosition();
                SetArrowPosition();
                SetArrowRotation();

                yield return null;
            }
        }

        private float CalculateDistane(Vector3 start, Vector3 finish)
        {
            return Vector3.Distance(start, finish);
        }

        private void SetMiddleTrajectoryPosition()
        {
            _middleTrajectoryPosition = new Vector3((_startTrajectoryPosition.x + _currentTarget.position.x) / 2, _startTrajectoryPosition.y + _hight, (_startTrajectoryPosition.z + _currentTarget.position.z) / 2);
        }

        private void CheckIsMoveUp()
        {
            if (CalculateDistane(transform.position, _currentTarget.position) < (CalculateDistane(_startTrajectoryPosition, _currentTarget.position) / 1.9f))
            {
                _isMoveUp = false;
            }
        }

        private void CalculateTargetPosition()
        {
            if (_isMoveUp)
            {
                _currentTargetPosition = _middleTrajectoryPosition;
            }
            else
            {
                _currentTargetPosition = _currentTarget.transform.position;
            }
        }

        private void SetArrowPosition()
        {
            transform.position = Vector3.MoveTowards(transform.position, _currentTargetPosition, _speedOfMove * Time.deltaTime);
        }

        private void SetArrowRotation()
        {
            Vector3 forward = new Vector3(transform.position.x - _currentTargetPosition.x, transform.position.y - _currentTargetPosition.y, transform.position.z - _currentTargetPosition.z) * -1;

            if (_isMoveUp = true & forward ! != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
            else if (_isMoveUp = false & forward! != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(forward), _speedOfRotation * Time.deltaTime);
            }
        }

        private void StopFly()
        {
            StopCoroutine(_fly);
            _fly = null;
        }
    }
}