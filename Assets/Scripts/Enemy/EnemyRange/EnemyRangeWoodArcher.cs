using System;
using System.Collections.Generic;
using Assets.Scripts.Enemy;
using UnityEngine;

internal class EnemyRangeWoodArcher : EnemyRange
{
    [SerializeField] private List<EnemyRangeArrow> _arrows;
    [SerializeField] private EnemyArrowSpawnPoint _spawnPoint;

    private EnemyRangeArrow _currentArrow;

    internal override void EnableArrow(Transform target)
    {
        SetEnemyRangeArrow();

        _currentArrow.gameObject.SetActive(true);
        _currentArrow.transform.position = _spawnPoint.transform.position;

        _currentArrow.StartFly(target);
    }

    internal void SetEnemyRangeArrow()
    {
        _currentArrow = null;

        foreach (EnemyRangeArrow arrow in _arrows)
        {
            if (arrow != null)
            {
                if (arrow.isActiveAndEnabled == false)
                {
                    _currentArrow = arrow;
                }
            }
        }
    }

    internal void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
}