using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal abstract class Weapon : MonoBehaviour
    {
        [SerializeField] private float _damage;
        [SerializeField] private float _attackSpeed;
        [SerializeField] protected LayerMask _layerMask;

        protected Coroutine AttackCoroutine;

        public bool CanAttack { protected set; get; }

        public float Damage => _damage;

        public float AttackSpeed => _attackSpeed;

        private void Awake()
        {
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

            yield return new WaitForSeconds(attackSpeed);

            CanAttack = true;
        }
    }
}