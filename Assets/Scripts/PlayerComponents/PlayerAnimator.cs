using Assets.Scripts.ConStants;
using Assets.Scripts.PlayerComponents.Weapons;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private Coroutine _animation;

        public bool IsPlayingAttackSword { get; private set; }
        public bool IsPlayingAttackBow { get; private set; }

        public void SetAnimatorSpeed(Vector3 movementVector, float moveSpeed)
        {
            float speed = Vector3.Magnitude(movementVector * moveSpeed);

            _animator.SetFloat(AnimatorHash.Speed, speed);
        }

        public void SetAnimatorAttackTrigger(Weapon weapon)
        {
            _animator.SetTrigger(weapon.Name + AnimatorHash.Attack);
        }

        public void SwordAttackAnimationStart()
        {
            IsPlayingAttackSword = true;
        }
        
        public void SwordAttackAnimationEnd()
        {
            IsPlayingAttackSword = false;
            Debug.Log("1");
        }

        private void StartCoroutine()
        {
            if (_animation != null)
            {
                StopCoroutine(_animation);
            }

            _animation = StartCoroutine(Animation());
        }

        private IEnumerator Animation()
        {
            IsPlayingAttackSword = true;

            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                yield return null;

            IsPlayingAttackSword = false;
        }
    }
}
