using UnityEngine;
using Assets.Scripts.Constants;

namespace Assets.Scripts.PlayerUnits
{
    [RequireComponent(typeof(Animator))]
    internal class UnitAnimator : MonoBehaviour
    {
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetMovingBool(bool isMoving)
        {
            _animator.SetBool(AnimatorHash.Moving, isMoving);
        }

        public void SetTriggerAttack()
        {
            _animator.SetTrigger(AnimatorHash.Attack);
        }
    }
}