using System.Net.Security;
using UnityEngine;

namespace Scripts.Enemy
{
    public class EnemyCollection : MonoBehaviour
    {
        [SerializeField] private MeleeEnemies _meleeEnemies;
        [SerializeField] private RangeEnemies _rangeEnemies;

        public GameObject GetRandomEnemy()
        {
            int numberOfMethod = Random.Range(1,3);

            switch (numberOfMethod)
            {
                case 1:
                    return GetMeleeEnemy();
                case 2:
                    return GetRangeEnemy();
                default:
                    break;
            }

            return null;
        }

        private GameObject GetMeleeEnemy()
        {
            return _meleeEnemies.GetMeleeEnemy();
        }

        private GameObject GetRangeEnemy()
        {
            return _rangeEnemies.GetRangeEnemy();
        }
    }
}