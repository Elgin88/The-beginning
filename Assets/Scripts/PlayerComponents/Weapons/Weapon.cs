using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    [RequireComponent(typeof(Collider))]
    internal abstract class Weapon : MonoBehaviour
    {
        [SerializeField] private float _damage;
        [SerializeField] private float _attackSpeed;

        protected Collider WeaponCollider;
        protected Coroutine AttackCoroutine;

        public bool CanAttack { protected set; get; }

        public float Damage => _damage;

        public float AttackSpeed => _attackSpeed;

        private void Start()
        {
            WeaponCollider = GetComponent<Collider>();

            WeaponCollider.enabled = false;

            CanAttack = true;
        }

        public virtual void Attack()
        {
            if (CanAttack)
                AttackCoroutine = StartCoroutine(AttackDelay(_attackSpeed));
        }

        private IEnumerator AttackDelay(float attackSpeed)
        {
            CanAttack = false;

            Debug.Log(attackSpeed);

            yield return new WaitForSeconds(attackSpeed);

            CanAttack = true;
        }
    }
}