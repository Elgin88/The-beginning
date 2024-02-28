using Assets.Scripts.Enemy;

internal class EnemyRangeWoodArcher : EnemyRange
{
    private float _startHealth = 50;
    private float _startDamage = 25;

    private void Awake()
    {
        Health = _startHealth;
        Damage = _startDamage;
    }
}