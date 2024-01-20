using Assets.Scripts.ConStants;
using Assets.Scripts.PlayerComponents.Weapons;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private string _clipName;
        private AnimatorClipInfo[] _currentClipInfo;

        private float _currentClipLength;

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

            SetCurrentClipInfo();

            if (_clipName == AnimatorHash.BowAttackString || _clipName == AnimatorHash.SwordAttackString)
            {
                _animator.SetFloat(AnimatorHash.AttackSpeed, CalculateAnimationSpeed(weapon));
            }
        }

        public void SetAnimatorChangeWeaponTrigger(Weapon weapon)
        {
            if (weapon is Bow)
                _animator.SetTrigger(AnimatorHash.EquipBow);
            else
                _animator.SetTrigger(AnimatorHash.EquipSword);
        }

        private void SetCurrentClipInfo()
        {
            _currentClipInfo = _animator.GetCurrentAnimatorClipInfo(0);

            _currentClipLength = _currentClipInfo[0].clip.length;

            _clipName = _currentClipInfo[0].clip.name;
        }

        private float CalculateAnimationSpeed(Weapon weapon)
        {
            return 1 / (weapon.AttackSpeed / _currentClipLength);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 200, 20), "Clip Name : " + _clipName);
            GUI.Label(new Rect(0, 30, 200, 20), "Clip Length : " + _currentClipLength);
        }
    }
}
