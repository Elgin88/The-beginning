using System.Collections.Generic;
using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Enemy;
using UnityEngine;

internal class EnemyRangeWoodArcher : EnemyRange
{
    [SerializeField] private List<EnemyRangeArrow> _arrows;
    [SerializeField] private EnemyArrowSpawnPoint _spawnPoint;
    [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;

    private EnemyRangeArrow _currentArrow;

    internal override void EnableArrow(Vector3 targetPosition)
    {
        SetEnemyRangeArrow();

        _currentArrow.gameObject.SetActive(true);
        _currentArrow.transform.position = _spawnPoint.transform.position;

        _currentArrow.StartFly(_enemyNextTargetFinder.CurrentTargetPosition, _enemyNextTargetFinder.CurrentTarget);
    }

    internal void InitMainBuilding(MainBuilding mainBuilding)
    {
        _enemyNextTargetFinder.InitMainBuilding(mainBuilding);
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

    internal override void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    internal override void SetRotation(Vector3 targetPosition)
    {
        transform.rotation = Quaternion.LookRotation(targetPosition);
    }
}