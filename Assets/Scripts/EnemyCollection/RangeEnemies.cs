using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Enemy
{
    public class RangeEnemies : MonoBehaviour
    {
        [SerializeField] private GameObject[] _rangeEnemies;
  
        public GameObject GetRangeEnemy()
        {
            foreach (var rangeEnemy in _rangeEnemies)
            {
                if (rangeEnemy.gameObject.activeSelf == false)
                {
                    rangeEnemy.gameObject.SetActive(true);

                    return rangeEnemy;
                }
            }

            return null;
        }
    }
}