using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scripts.Enemy
{
    public class MeleeEnemies : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _meleeEnemies;

        public GameObject GetMeleeEnemy()
        {
            foreach (GameObject enemy in _meleeEnemies)
            {
                if (enemy.activeSelf == false & enemy != null)
                {
                    enemy.gameObject.SetActive(true);
                    return enemy;
                }
            }

            return null;
        }

        private void Start()
        {
            
        }
    }
}