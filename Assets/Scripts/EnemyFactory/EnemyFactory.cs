using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    internal class EnemyFactory: MonoBehaviour
    {
        [SerializeField] private MeleeEnemy _meleeEnemy;
        [SerializeField] private RangeEnemy _rangeEneny;

        [Inject] private DiContainer _currentEnemyDI;

        public void SpawnRandom(Vector3 position)
        {
            switch (Random.Range(1, 3))
            {
                case 1:
                    SpawnMeleeEnemy(position);
                    break;

                case 2:
                    SpawnRangeEnemy(position);
                    break;

                default:
                    break;
            }
        }

        internal void SpawnMeleeEnemy(Vector3 position)
        {
            _currentEnemyDI.InstantiatePrefab(_meleeEnemy, position, Quaternion.identity, null);
        }

        internal void SpawnRangeEnemy(Vector3 position)
        {
            _currentEnemyDI.InstantiatePrefab(_rangeEneny, position, Quaternion.identity, null);
        }
    }
}
