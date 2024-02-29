using Assets.Scripts.Enemy;
using UnityEngine;

internal class EnemyRangeWoodArcher : EnemyRange
{
    [SerializeField] private float _startHealth;
    [SerializeField] private float _startDamage;

    private void Awake()
    {
        Health = _startHealth;
        Damage = _startDamage;
    }
}