using UnityEngine;

namespace Assets.Scripts.Movement
{
    internal class Moveable 
    {
        private float _moveSpeed;
        private float _rotationSpeed;
        private LayerMask _layerMask;
        private Transform _transform;

        private float _alignmentSpeed = 2.5f;
        private float _desiredHeight = 1.05f;
        private float _rayDistance = 5f;

        public Moveable(IMoveable target, LayerMask mask)
        {
            _transform = target.Transform;
            _moveSpeed = target.Speed;
            _rotationSpeed = target.RotationSpeed;
            _layerMask = mask;
        }

        public void Move(Vector2 direction)
        {
            float scaledMoveSpeed = _moveSpeed * Time.fixedDeltaTime;
            Vector3 movementVector = new Vector3(direction.x, 0, direction.y);

            _transform.position += movementVector * scaledMoveSpeed;

            SurfaceAlignment(movementVector);
        }

        private void SurfaceAlignment(Vector3 movementVector)
        {
            Ray ray = new Ray(_transform.position, -_transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _rayDistance, _layerMask.value))
            {
                if (hit.distance > _desiredHeight)
                {
                    _transform.position = Vector3.Lerp(_transform.position, hit.point, _alignmentSpeed * Time.fixedDeltaTime);
                }

                Rotate(movementVector, hit.normal);
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