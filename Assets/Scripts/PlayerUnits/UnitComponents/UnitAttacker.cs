using Assets.Scripts.GameLogic;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    [RequireComponent(typeof(UnitMover))]
    internal class UnitAttacker : MonoBehaviour
    {
        private float _radius = 5f;
        private LayerMask _layerMask;

        private ClosestTargetFinder _closestTargetFinder;
        private UnitMover _mover;

        private float _attackRange;
        private IDamageable _target;

        private void Start()
        {
            _mover = GetComponent<UnitMover>();
            _closestTargetFinder = new ClosestTargetFinder(_radius, _layerMask);
        }

        private void Attack()
        {

        }

        public void FindTarget()
        {
            if (_closestTargetFinder.TryFindTarget(transform.position, out _target))
            {
                if (Vector3.Distance(transform.position, _target.Transform.position) <= _attackRange) 
                { 
                    Attack();
                }
                else
                {
                    _mover.Move(_target.Transform.position);
                }
            }
        }
    }
}
