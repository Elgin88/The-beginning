using Assets.Scripts.ConStants;
using Assets.Scripts.PlayerComponents.Weapons;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        public void SetAnimatorSpeed(Vector3 movementVector, float moveSpeed)
        {
            float speed = Vector3.Magnitude(movementVector * moveSpeed);

            _animator.SetFloat(AnimatorHash.Speed, speed);
        }

        public void SetAnimatorAttackTrigger(Weapon weapon)
        {
            if (weapon is Bow)
                _animator.SetTrigger(AnimatorHash.BowAttack);
            else
                _animator.SetTrigger(AnimatorHash.SwordAttack);
        }

        public void SetAnimatorChangeWeaponTrigger(Weapon weapon)
        {
            if (weapon is Bow)
                _animator.SetTrigger(AnimatorHash.EquipBow);
            else
                _animator.SetTrigger(AnimatorHash.EquipSword);
        }
    }
}
