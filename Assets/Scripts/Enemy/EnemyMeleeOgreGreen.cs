using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyMeleeOgreGreen : EnemyMelee
    {
        [SerializeField] private float _startHealth;
        [SerializeField] private float _startDamage;

        private void Awake()
        {
            Health = _startHealth;
            Damage = _startDamage;
        }
    }
}