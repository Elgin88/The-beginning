using UnityEngine;

namespace Scripts.Enemy
{
    public class BankOfEnemies : MonoBehaviour
    {
        [SerializeField] private MeleeEnemies _meleeEnemies;
        [SerializeField] private RangeEnemies _rangeEnemies;

        public GameObject GetMeleeEnemy()
        {
            return _meleeEnemies.GetMeleeEnemy();
        }
    }
}