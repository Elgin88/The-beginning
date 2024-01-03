using UnityEngine;

namespace Scripts.Enemy
{
    public class MeleeEnemies : MonoBehaviour
    {
        [SerializeField] private MeleeEnemy[] _meleeEnemies;

        public GameObject GetEnemy()
        {
            foreach (MeleeEnemy enemy in _meleeEnemies)
            {
                if (enemy.gameObject.activeSelf == false)
                {
                    enemy.gameObject.SetActive(true);

                    return enemy.gameObject;
                }
            }

            return null;
        }
    }
}