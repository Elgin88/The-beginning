using UnityEngine;

namespace Assets.Scripts.GameLogic.Damageable
{
    internal interface IDamageable
    {
        public bool IsPlayerObject { get; }

        public bool IsDead { get; }

        public Transform Transform { get; }

        public void TakeDamage(float damage);
    }
}