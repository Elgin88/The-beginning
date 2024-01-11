using Assets.Scripts.PlayerComponents;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    internal class PlayerMovement
    {
        private static LayerMask _groundMask = LayerMask.GetMask("Water");

        private float _moveSpeed;
        private float _slowSpeed;
        private float _rotationSpeed;
        private Transform _transform;
        private PlayerAnimator _animator;

        private float _alignmentSpeed = 2.5f;
        private float _desiredHeight = 1.08f;
        private float _rayDistance = 5f;

        public PlayerMovement(Player player, PlayerAnimator animator)
        {
            _moveSpeed = player.Speed;
            _rotationSpeed = player.RotationSpeed;
            _transform = player.transform;
            _animator = animator;

            _slowSpeed = _moveSpeed;
        }

        public void Move(Vector2 direction)
        {
            if (_animator.IsPlayingAttackSword)
            {
                _moveSpeed = 0;
                Debug.Log(_moveSpeed);
            }
            else
            {
                _moveSpeed = _slowSpeed;

            }

            float scaledMoveSpeed = _moveSpeed * Time.fixedDeltaTime;
            Vector3 movementVector = new Vector3(direction.x, 0, direction.y);
            _animator.SetAnimatorSpeed(movementVector, _moveSpeed);
            
            _transform.position += movementVector * scaledMoveSpeed;

            SurfaceAlignment(movementVector);
        }

        private void SurfaceAlignment(Vector3 movementVector)
        {
            Ray ray = new Ray(_transform.position, -_transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _rayDistance, _groundMask))
            {
                Rotate(movementVector, hit.normal);

                if (hit.distance > _desiredHeight)
                {
                    _transform.position = Vector3.Lerp(_transform.position, hit.point, _alignmentSpeed * Time.fixedDeltaTime);
                }
            }
        }

        private void Rotate(Vector3 movementVector, Vector3 groundNormal)
        {
            if (movementVector.sqrMagnitude > 0)
            {
                Quaternion surfaceAlignmentRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
                Quaternion directionRotation = Quaternion.LookRotation(movementVector);

                Quaternion combinedRotation = surfaceAlignmentRotation * directionRotation;

                _transform.rotation = Quaternion.Slerp(_transform.rotation, combinedRotation, _rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
}