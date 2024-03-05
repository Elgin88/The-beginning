using System;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    public class EnemyRangeArrow : MonoBehaviour
    {
        [SerializeField] private Transform _finish;
        [SerializeField] private float _speedOfMove;
        [SerializeField] private float _speedOfRotation;
        [SerializeField] private float _hight;

        private Vector3 _currentTargetPosition;
        private Vector3 _startTrajectoryPosition;
        private Vector3 _middleTrajectoryPosition;
        private bool _isMoveUp;

        private void OnEnable()
        {
            transform.rotation = Quaternion.LookRotation(_finish.position);
            _startTrajectoryPosition = transform.position;
            _isMoveUp = true;
        }

        private void Update()
        {
            CalculateDistane(_startTrajectoryPosition, _finish.position);
            SetMiddleTrajectoryPosition();
            CheckIsMoveUp();
            CalculateTargetPosition();
            SetArrowPosition();
            SetArrowRotation();

            if (transform.position == _finish.transform.position)
            {
                transform.position = _startTrajectoryPosition;
                _isMoveUp = true;
            }
        }

        private float CalculateDistane(Vector3 start, Vector3 finish)
        {
            return Vector3.Distance(start, finish);
        }

        private void SetMiddleTrajectoryPosition()
        {
            _middleTrajectoryPosition = new Vector3((_startTrajectoryPosition.x + _finish.position.x) / 2, _startTrajectoryPosition.y + _hight, (_startTrajectoryPosition.z + _finish.position.z) / 2);
        }

        private void CheckIsMoveUp()
        {
            if (CalculateDistane(transform.position, _finish.position) < (CalculateDistane(_startTrajectoryPosition, _finish.position) / 1.9f))
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
                _currentTargetPosition = _finish.transform.position;
            }
        }

        private void SetArrowPosition()
        {
            transform.position = Vector3.MoveTowards(transform.position, _currentTargetPosition, _speedOfMove * Time.deltaTime);
        }

        private void SetArrowRotation()
        {
            Vector3 forward = new Vector3(transform.position.x - _currentTargetPosition.x, transform.position.y - _currentTargetPosition.y, transform.position.z - _currentTargetPosition.z);
            transform.rotation = Quaternion.LookRotation(- forward);
        }
    }
}