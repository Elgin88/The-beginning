using UnityEngine;

namespace Assets.Scripts.GameLogic.Damageable
{
    internal interface IDamageable
    {
        public Transform Transform { get; }

        public void TakeDamage(int damage);
    }
}