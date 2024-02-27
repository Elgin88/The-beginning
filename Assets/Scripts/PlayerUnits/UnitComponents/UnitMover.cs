using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits
{
    internal class UnitMover
    {
        private NavMeshAgent _navMesh;
        private UnitAnimator _animator;

        public bool IsMoving { get; private set; }

        public UnitMover(NavMeshAgent navMesh, UnitAnimator animator) 
        { 
            _navMesh = navMesh;
            _animator = animator;
        }

        public void Move(Vector3 position)
        {
            _navMesh.SetDestination(position);
            SetIsMoving(true);
        }

        public void StopMoving()
        {
            SetIsMoving(false);
            _navMesh.ResetPath();
        }

        private void SetIsMoving(bool isMoving)
        {
            IsMoving = isMoving;
            _animator.SetMovingBool(IsMoving);
        }
    }
}
