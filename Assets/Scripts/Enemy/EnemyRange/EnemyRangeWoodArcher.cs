using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enemy;

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