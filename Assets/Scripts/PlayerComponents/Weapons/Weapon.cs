using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    [RequireComponent(typeof(Collider))]
    internal abstract class Weapon : MonoBehaviour
    {
        [SerializeField] protected LayerMask LayerMask;

        [SerializeField] private float _damage;
        [SerializeField] private float _attackSpeed;

        protected Coroutine AttackCoroutine;
        protected bool CanAttack = true;
        protected float TimePast;

        private Collider _weaponCollider;

        private void Start()
        {
            _weaponCollider = GetComponent<Collider>();

            _weaponCollider.enabled = false;
        }

        public float Damage => _damage;

        public float AttackSpeed => _attackSpeed;

        public virtual void Attack()
        {
            if (AttackCoroutine != null)
            {
                TimePast = 0;
                StopCoroutine(AttackCoroutine);
            }

            AttackCoroutine =  StartCoroutine(AttackDelay(_attackSpeed));
        }

        private IEnumerator AttackDelay(float attackSpeed)
        {
            CanAttack = false;

            var attackDelay = new WaitForSeconds(attackSpeed);

            yield return attackDelay;

            CanAttack = true;
        }
    }
}