using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    [RequireComponent(typeof(Collider))]
    internal class Sword : Weapon
    {
        private Coroutine _attackCoroutine;
        private Collider _swordCollider;

        private void Start()
        {
            _swordCollider = GetComponent<Collider>();

            _swordCollider.enabled = false;
        }

        public override void Attack()
        {
            _swordCollider.enabled = true;

            if (_attackCoroutine != null )
            {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(AttackDelay(AttackSpeed));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == _layerMask && other.gameObject.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(Damage);
            }
        }

        private IEnumerator AttackDelay(float attackSpeed)
        {
            base.Attack();

            yield return new WaitForSeconds(attackSpeed);

            _swordCollider.enabled = false;
        }
    }
}
