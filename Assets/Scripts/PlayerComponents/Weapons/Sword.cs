using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Sword : Weapon
    {
        private Coroutine _attackCoroutine;

        public override void Attack()
        {
            WeaponCollider.enabled = true;

            if (_attackCoroutine != null )
            {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(AttackDelay(AttackSpeed));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable enemy))
            {
                enemy.TakeDamage(Damage);
            }
        }

        private IEnumerator AttackDelay(float attackSpeed)
        {
            base.Attack();

            yield return new WaitForSeconds(attackSpeed);

            WeaponCollider.enabled = false;
        }
    }
}
