using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    internal class SurfaceAlignment
    {
        private static LayerMask _groundMask = LayerMask.GetMask("Water");

        private float _alignmentSpeed = 2.5f;
        private float _desiredHeight = 1.08f;
        private float _rayDistance = 5f;
        private float _rotationSpeed = 5f;

        private IMoveable _target;

        public SurfaceAlignment(IMoveable target)
        {
            _target = target;
        }

        public void Align(Vector3 movementVector)
        {
            Ray ray = new Ray(_target.Transform.position, -_target.Transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _rayDistance, _groundMask))
            {
                Rotate(movementVector, hit.normal);

                if(hit.normal.y < 0.78f)
                {
                    _target.Transform.position -= movementVector / 8;
                }

                if (hit.distance > _desiredHeight)
                {
                    _target.Transform.position = Vector3.Lerp(_target.Transform.position, hit.point, _alignmentSpeed * Time.fixedDeltaTime);
                }
            }
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
