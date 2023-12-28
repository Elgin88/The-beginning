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
        private Vector3 _mainEnemyInSpawnPosition;
        private float _currentEnemyPositionX;
        private float _currentEnemyPositionY;
        private float _currentEnemyPositionZ;

        public void CreateMinorEnemies(Vector3 mainEnemyInSpawnPosition)
        {
            _mainEnemyInSpawnPosition = mainEnemyInSpawnPosition;

            for (int i = 0; i < _enemyCount; i++)
            {
                _currentEnemy = _enemyCollection.GetRandomEnemy();

                if (_currentEnemy != null)
                {
                    CalculateStartPosition();
                    SetStartPosition();
                }
            }
        }

        private void CalculateStartPosition()
        {
            bool isWork = true;
            float radius;

            while (isWork)
            {
                _currentEnemyPositionX = _mainEnemyInSpawnPosition.x + Random.Range(-_maxSpawnRange, _maxSpawnRange);
                _currentEnemyPositionZ = _mainEnemyInSpawnPosition.z + Random.Range(-_maxSpawnRange, _maxSpawnRange);

                radius = Mathf.Sqrt(Mathf.Pow(_currentEnemyPositionX - _mainEnemyInSpawnPosition.x, 2) + Mathf.Pow(_currentEnemyPositionZ - _mainEnemyInSpawnPosition.z, 2));

                if (radius >= _minSpawnRange & radius <= _maxSpawnRange)
                {
                    isWork = false;
                }            
            }

            _currentEnemyPositionY = _mainEnemyInSpawnPosition.y;
        }

        private void SetStartPosition()
        {
            _currentEnemy.transform.position = new Vector3(_currentEnemyPositionX, _currentEnemyPositionY, _currentEnemyPositionZ);
        }
    }
}