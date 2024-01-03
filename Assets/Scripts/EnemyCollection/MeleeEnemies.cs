using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class MeleeEnemies : MonoBehaviour
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