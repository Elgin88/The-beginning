using System.Collections;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.EnemyNamespace
{
    public class EnemyRangeArrow : MonoBehaviour
    {
        [SerializeField] private EnemyRangeWoodArcher _enemyRangeWoodArcher;
        [SerializeField] private LayerMask _layerMask;

        private Coroutine _fly;
        private bool _isMoveUp;
        private float _speedOfMove = 20;
        private float _speedOfRotation = 20;
        private float _hight = 0.5f;

        internal void StartFly(GameObject target)
        {
            _fly = StartCoroutine(Fly(target));
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.includeLayers == _layerMask)
            {
                Debug.Log(collider.name);

                collider.GetComponent<IDamageable>().TakeDamage(_enemyRangeWoodArcher.GetComponent<IEnemy>().Damage);
                StopFly();
                gameObject.SetActive(false);
            }
        }

        private IEnumerator Fly(GameObject target)
        {
            Vector3 startPosition = transform.position;

            while (transform.position != target.transform.position)
            {
                CalculateDistane(startPosition, target.transform.position);
                SetMiddleTrajectoryPosition(startPosition, target.transform.position, out Vector3 middlePosition);
                CheckIsMoveUp(startPosition, target.transform.position);
                CalculateTargetPosition(middlePosition, target.transform.position, out Vector3 currentTargetPosition);
                SetArrowPosition(currentTargetPosition);
                SetArrowRotation(currentTargetPosition);

                yield return null;
            }

            StopFly();
            gameObject.SetActive(false);
        }

        private float CalculateDistane(Vector3 start, Vector3 finish)
        {
            return Vector3.Distance(start, finish);
        }

        private void SetMiddleTrajectoryPosition(Vector3 startPosition, Vector3 targetPosition, out Vector3 middlePosition)
        {
            middlePosition = new Vector3((startPosition.x + targetPosition.x) / 2, startPosition.y + _hight, (startPosition.z + targetPosition.z) / 2);
        }

        private void CheckIsMoveUp(Vector3 startPosition, Vector3 targetPosition)
        {
            if (CalculateDistane(transform.position, targetPosition) < (CalculateDistane(startPosition, targetPosition) / 1.9f))
            {
                _isMoveUp = false;
            }
        }

        private void CalculateTargetPosition(Vector3 middlePosition, Vector3 targetPosition, out Vector3 currentTargetPosition)
        {
            if (_isMoveUp)
            {
                currentTargetPosition = middlePosition;
            }
            else
            {
                currentTargetPosition = targetPosition;
            }
        }

        private void SetArrowPosition(Vector3 targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _speedOfMove * Time.deltaTime);
        }

        private void SetArrowRotation(Vector3 targetPosition)
        {
            Vector3 forward = new Vector3(transform.position.x - targetPosition.x, transform.position.y - targetPosition.y, transform.position.z - targetPosition.z) * -1;

            if (_isMoveUp = true & forward != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
            else if (_isMoveUp = false & forward!= Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(forward), _speedOfRotation * Time.deltaTime);
            }
        }

        private void StopFly()
        {
            StopCoroutine(_fly);
        }
    }
}