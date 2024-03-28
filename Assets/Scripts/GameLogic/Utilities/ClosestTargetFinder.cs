using Assets.Scripts.GameLogic.Damageable;
using System;
using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    internal class ClosestTargetFinder
    {
        private float _radius;
        private LayerMask _layerMask;
        private Collider[] _hitColliders;

        public ClosestTargetFinder(float radius, LayerMask layerMask)
        {
            _radius = radius;
            _layerMask = layerMask;
        }

        public bool TryFindTarget(Vector3 currentPosition, out IDamageable target)
        {
            _hitColliders = Physics.OverlapSphere(currentPosition, _radius, _layerMask);

            if (_hitColliders.Length > 0)
            {
                Array.Sort(_hitColliders, (Collider x, Collider y)
                    => Vector3.Distance(currentPosition, x.transform.position)
                    .CompareTo(Vector3.Distance(currentPosition, y.transform.position)));

                if (_hitColliders[0].TryGetComponent<IDamageable>(out IDamageable enemy))
                {
                    target = enemy;

                    return true;
                }
            }

            target = null;

            return false;
        }
    }
}
