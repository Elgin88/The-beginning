using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerMover 
    {
        private float _moveSpeed;
        private float _rotationSpeed;
        private LayerMask _layerMask;
        private Rigidbody _rigidbody;
        private Transform _transform;

        private float _desiredHeight = 0.2f;

        private Vector3 _offset = new Vector3(0, 0.1f, 0);

        public PlayerMover(IMovable target, LayerMask mask)
        {
            _transform = target.Transform;
            _rigidbody = target.Rigidbody;
            _moveSpeed = target.Speed;
            _rotationSpeed = target.RotationSpeed;
            _layerMask = mask;
        }

        public void Move(Vector2 direction)
        {
            float scaledMoveSpeed = _moveSpeed * Time.fixedDeltaTime;
            Vector3 movementVector = new Vector3(direction.x, 0, direction.y);
            Vector3 counterMovement = new Vector3(-_rigidbody.velocity.x, 0, -_rigidbody.velocity.z);

            _transform.position += movementVector * scaledMoveSpeed;

            //_rigidbody.AddForce(movementVector * scaledMoveSpeed);

            //Rotate(movementVector);
            SurfaceAlignment();
        }

        private void Rotate(Vector3 movementVector)
        {
            if (movementVector.sqrMagnitude == 0)
                return;

            Quaternion rotation = Quaternion.LookRotation(movementVector);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, rotation, _rotationSpeed);
        }

        private void SurfaceAlignment()
        {
            Ray ray = new Ray(_transform.position, -_transform.up);
            RaycastHit hit;

            Debug.DrawRay(_transform.position, -_transform.up * 1.1f, Color.red);

            if (Physics.Raycast(ray, out hit, 1.1f, _layerMask.value))
            {
                Debug.Log("Hit");

                // Set player's position to be on the ground hit
                Vector3 targetPosition = new Vector3(_transform.position.x, hit.point.y + 1f, _transform.position.z);
                _transform.position = Vector3.Lerp(_transform.position, targetPosition, 0.15f);

                // Rotate player to match the ground normal
                Quaternion rotationReference = Quaternion.FromToRotation(Vector3.up, hit.normal);
                _transform.rotation = Quaternion.Lerp(_transform.rotation, rotationReference, 0.15f);
            }
            else
            {
                Debug.Log("NoHit");

                // Adjust the player's position when not hitting the ground
                _transform.position -= _offset;
            }
        }
    }
}