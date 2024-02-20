namespace Assets.Scripts.Enemy
{
    internal class EnemyMeleeOgreGreen : EnemyMelee
    {
        private float _startHealth = 100;
        private float _startDamage = 10;

        private void Awake()
        {
            Health = _startHealth;
            Damage = _startDamage;
        }
    }
}