using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerMovement
    {
        private static LayerMask _groundMask = LayerMask.GetMask("Water");

        private float _attackMoveSpeed;
        private PlayerAnimator _animator;
        private Player _player;

        private bool _isAttacking;
        private float _alignmentSpeed = 2.5f;
        private float _desiredHeight = 1.08f;
        private float _rayDistance = 5f;

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

            SurfaceAlignment(movementVector);
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
            Debug.DrawLine(_player.transform.position, target.position, Color.red);

            //Quaternion directionRotation = Quaternion.LookRotation(target.position);

            //_player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, directionRotation, Time.fixedDeltaTime);

            _player.transform.LookAt(target.position);
        }

        private void SurfaceAlignment(Vector3 movementVector)
        {
            Ray ray = new Ray(_player.transform.position, -_player.transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _rayDistance, _groundMask))
            {
                Rotate(movementVector, hit.normal);

                if (hit.distance > _desiredHeight)
                {
                    _player.transform.position = Vector3.Lerp(_player.transform.position, hit.point, _alignmentSpeed * Time.fixedDeltaTime);
                }
            }
        }

        private void Rotate(Vector3 movementVector, Vector3 groundNormal)
        {
            if (movementVector.sqrMagnitude > 0 && _isAttacking == false)
            {
                Quaternion surfaceAlignmentRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
                Quaternion directionRotation = Quaternion.LookRotation(movementVector);

                Quaternion combinedRotation = surfaceAlignmentRotation * directionRotation;

                _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, combinedRotation, _player.RotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
}