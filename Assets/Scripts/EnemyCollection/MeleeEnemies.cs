using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Enemy
{
    public class MeleeEnemies : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _meleeEnemies;

        public GameObject GetEnemy()
        {
            foreach (GameObject enemy in _meleeEnemies)
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