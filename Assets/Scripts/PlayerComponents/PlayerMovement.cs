using Assets.Scripts.GameLogic;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerMovement : IMoveable
    {
        private float _attackMoveSpeed;
        private PlayerAnimator _animator;
        private Player _player;

        private bool _isAttacking;

        public Transform Transform => _player.transform;

        public float MoveSpeed => _player.Speed;

        public SurfaceAlignment SurfaceAlignment => new SurfaceAlignment(this);

        public PlayerMovement(Player player, PlayerAnimator animator)
        {
            _player = player;
            _animator = animator;

            _attackMoveSpeed = 0;
        }

        public void Move(Vector2 direction)
        {
            float moveSpeed = _isAttacking ? _attackMoveSpeed : _player.Speed;
            float scaledMoveSpeed = moveSpeed * Time.fixedDeltaTime;
            Vector3 movementVector = new Vector3(direction.x, 0, direction.y);

            _animator.SetAnimatorSpeed(movementVector, _player.Speed);

            _player.transform.position += movementVector * scaledMoveSpeed;

            SurfaceAlignment.Align(movementVector);
        }

        public void StopMove()
        {
            _isAttacking = true;
        }

        public void StartMove()
        {
            _isAttacking = false;
        }

        public void RotateTowards(Transform target, Vector3 offset)
        {
            Vector3 directionToTarget = target.position - _player.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
            offset = new Vector3(_player.transform.rotation.x, offset.y, _player.transform.rotation.z);

            _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, targetRotation * Quaternion.Euler(offset), Time.fixedDeltaTime / 2);
        }
    }
}