using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyMelleeOrcGreen : EnemyMelee
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;

        internal override void InitMainBuilding(MainBuilding mainBuilding)
        {
            _enemyNextTargetFinder.InitMainBuilding(mainBuilding);
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
}