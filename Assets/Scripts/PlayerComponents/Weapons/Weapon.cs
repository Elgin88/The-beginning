using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    [RequireComponent(typeof(Collider))]
    internal abstract class Weapon : MonoBehaviour
    {
        [SerializeField] private float _damage;
        [SerializeField] private float _attackSpeed;
        [SerializeField] private string _name;

        [SerializeField] protected LayerMask LayerMask;

        private Collider _weaponCollider;

        protected Coroutine AttackCoroutine;
        protected bool CanAttack = true;
        protected float TimePast;

        public string Name => _name;

        public float Damage => _damage;

        public float AttackSpeed => _attackSpeed;

        private void Start()
        {
            _weaponCollider = GetComponent<Collider>();

            _weaponCollider.enabled = false;
        }

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