using UnityEngine;

namespace Scripts.Enemy
{
    public class RangeEnemies : MonoBehaviour
    {
        [SerializeField] private GameObject[] _rangeEnemies;
  
        public GameObject GetEnemy()
        {
            foreach (GameObject enemy in _rangeEnemies)
            {
                if (enemy.gameObject.activeSelf == false)
                {
                    enemy.gameObject.SetActive(true);

                    return enemy;
                }
            }

            return null;
        }
    }
}