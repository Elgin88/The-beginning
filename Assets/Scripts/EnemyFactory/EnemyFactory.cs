using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyFactory: MonoBehaviour
    {
        [SerializeField] private MeleeEnemy _meleeEnemy;
        [SerializeField] private RangeEnemy _rangeEneny;

        public void SpawnRandom(Vector3 position)
        {
            switch (Random.Range(1, 2))
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
            Instantiate(_meleeEnemy, position, Quaternion.identity);
        }

        internal void SpawnRangeEnemy(Vector3 position)
        {
            Instantiate(_rangeEneny, position, Quaternion.identity);
        }
    }
}
