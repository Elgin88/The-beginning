using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Enemy
{
    public class EnemyCreatorSpawn : MonoBehaviour
    {
        [SerializeField] private float _minSpawnRange;
        [SerializeField] private float _maxSpawnRange;
        [SerializeField] private float _enemyCount;
        [SerializeField] private EnemyCollection _enemyCollection;

        public void CreateMinorEnemies(GameObject mainEnemy)
        {
            _enemyCollection.GetRandomEnemy();
        }
    }
}