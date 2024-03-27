using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;

namespace Assets.Scripts.EnemyNamespace
{
    internal class EnemyMelleeOrcGreen : EnemyMelee
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;

        internal override void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        internal override void SetRotationToTarget(Vector3 targetPosition)
        {
            if (targetPosition != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(targetPosition);
            }
        }
    }
}