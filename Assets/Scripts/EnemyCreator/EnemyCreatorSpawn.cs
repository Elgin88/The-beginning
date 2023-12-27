using System;
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

        private GameObject _currentEnemy;
        private Vector3 _mainEnemyPosition;
        private float _currentEnemyPositionX;
        private float _currentEnemyPositionY;
        private float _currentEnemyPositionZ;

        public void CreateMinorEnemies(Vector3 mainEnemyPosition)
        {
            _mainEnemyPosition = mainEnemyPosition;

            for (int i = 0; i < _enemyCount; i++)
            {
                _currentEnemy = _enemyCollection.GetRandomEnemy();

                if (_currentEnemy != null)
                {
                    CalculateCurrentEnemyPosition();
                    SetCurrentEnemyPosition();
                }
            }
        }

        private void CalculateCurrentEnemyPosition()
        {
            Debug.Log("Дописать здесь");
        }

        private void SetCurrentEnemyPosition()
        {
            _currentEnemy.transform.position = new Vector3(_currentEnemyPositionX, _currentEnemyPositionY, _currentEnemyPositionZ);
        }
    }
}