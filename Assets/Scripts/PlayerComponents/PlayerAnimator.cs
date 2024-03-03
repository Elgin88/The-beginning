using System.Collections;
using UnityEngine;
using Assets.Scripts.AnimatorScripts.Player;
using Assets.Scripts.Constants;
using Assets.Scripts.PlayerComponents.Weapons;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private AnimatorTriggerConfiguration _triggerConfig = new AnimatorTriggerConfiguration();

        private AnimatorClipInfo[] _currentClipInfo;
        private float _currentClipLength;
        private float _animationUpdateTime = 0.5f;

        private Coroutine _animatorUpdate;

        public void SetAnimatorSpeed(Vector3 movementVector, float moveSpeed)
        {
            float speed = Vector3.Magnitude(movementVector * moveSpeed);

            _animator.SetFloat(AnimatorHash.Speed, speed);
        }

        public void SetAnimatorAttackTrigger(Weapon weapon)
        {
            if (_animatorUpdate != null)
            {
                StopCoroutine(_animatorUpdate);
            }

            _animatorUpdate = StartCoroutine(AnimatorUpdate(weapon));
        }

        public void SetAnimatorChangeWeaponTrigger(Weapon weapon)
        {
            _animator.SetTrigger(_triggerConfig.GetChangeWeaponTrigger(weapon.GetType()));
        }

        private IEnumerator AnimatorUpdate(Weapon weapon)
        {
            _animator.SetTrigger(_triggerConfig.GetAttackTrigger(weapon.GetType()));

            _animator.Update(0);

            yield return new WaitForSeconds(_animationUpdateTime);

            SetCurrentClipInfo();

            _animator.SetFloat(AnimatorHash.AttackSpeed, CalculateAnimationSpeed(weapon));
        }

        private void SetCurrentClipInfo()
        {
            _currentClipInfo = _animator.GetCurrentAnimatorClipInfo(0);

            _currentClipLength = _currentClipInfo[0].clip.length;
        }

        private float CalculateAnimationSpeed(Weapon weapon)
        {
            return 1 / (weapon.AttackSpeed / _currentClipLength);
        }
    }
}
