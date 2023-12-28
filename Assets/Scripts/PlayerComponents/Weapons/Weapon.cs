using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    [RequireComponent(typeof(Collider))]
    internal abstract class Weapon : MonoBehaviour
    {
        [SerializeField] private float _damage;
        [SerializeField] private float _attackSpeed;

        [SerializeField] protected LayerMask _layerMask;

        private Collider _weaponCollider;

        private void Start()
        {
            _weaponCollider = GetComponent<Collider>();
        }

        public float Damage => _damage;

        public float AttackSpeed => _attackSpeed;

        public abstract void Attack();
    }
}