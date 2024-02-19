using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    internal class SurfaceAlignment
    {
        private static LayerMask _groundMask = LayerMask.GetMask("Water");

        private float _alignmentSpeed = 6f;
        private float _desiredHeight = 0.1f;
        private float _rayDistance = 5f;
        private float _rotationSpeed = 5f;
        private float _maxSlopeNormal = 0.8f;
        private float _minSlopeHeight = 1.5f;

        private IMoveable _target;

        public bool CanWalk { get; private set; }

        public SurfaceAlignment(IMoveable target)
        {
            _target = target;
            CanWalk = true;
        }

        public void Align(Vector3 movementVector)
        {
            Ray ray = new Ray(_target.Transform.position, -_target.Transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _rayDistance, _groundMask))
            {
                Rotate(movementVector, hit.normal);

                if (hit.distance > _desiredHeight)
                {
                    _target.Transform.position = Vector3.Lerp(_target.Transform.position, hit.point, _alignmentSpeed * Time.fixedDeltaTime);
                }
            }
        }

        public bool CanWalkOnSlope()
        {
            Vector3 pos = _target.Transform.position + _target.Transform.forward + Vector3.up;
            RaycastHit hit;
            Ray ray1 = new Ray(pos, Vector3.down);

            if (Physics.Raycast(ray1, out hit, _rayDistance, _groundMask))
            {
                if (hit.normal.y < _maxSlopeNormal || Vector3.Distance(pos, hit.point) > _minSlopeHeight)
                {
                    return false;
                }
            }

            return true;
        }

        private void Rotate(Vector3 movementVector, Vector3 groundNormal)
        {
            if (movementVector.sqrMagnitude > 0f)
            {
                Quaternion surfaceAlignmentRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
                Quaternion directionRotation = Quaternion.LookRotation(movementVector);

                Quaternion combinedRotation = surfaceAlignmentRotation * directionRotation;

                _target.Transform.rotation = Quaternion.Slerp(_target.Transform.rotation, combinedRotation, _rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
