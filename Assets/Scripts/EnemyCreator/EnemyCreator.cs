using System.Collections;
using UnityEngine;

namespace Scripts.Enemy
{
    public class EnemyCreator : MonoBehaviour
    {
        [SerializeField] private GameObject _playerMainBilding;
        [SerializeField] private EnemyCollection _enemyCollection;
        [SerializeField] private float _minRadiusSpawn;
        [SerializeField] private float _maxRadiusSpawn;
        [SerializeField] private float _delayBetweenMainSpawn;
        [SerializeField] private float _minMinorSpawnRange;
        [SerializeField] private float _maxMinorSpawnRange;
        [SerializeField] private float _minorEnemyCount;

        private GameObject _currentMainEnemy;
        private GameObject _currentMinorEnemy;
        private float _currentMainEnemyPositionX;
        private float _currentMainEnemyPositionY;
        private float _currentMainEnemyPositionZ;
        private float _currentMinorEnemyPositionX;
        private float _currentMinorEnemyPositionY;
        private float _currentMinorEnemyPositionZ;
        private WaitForSeconds _delayBetweenMainSpawnWFS;
        private Coroutine _createEnemies;

        private void Start()
        {
            _delayBetweenMainSpawnWFS = new WaitForSeconds(_delayBetweenMainSpawn);
            _createEnemies = StartCoroutine(CreateEnemies());
        }

        private IEnumerator CreateEnemies()
        {
            while (true)
            {
                _currentMainEnemy = _enemyCollection.GetRandomEnemy();

                if (_currentMainEnemy == null)
                {
                    StopCoroutine(_createEnemies);
                    yield break;
                }

                CreateMainEnemy();
                CreateMinorEnemies();

                yield return _delayBetweenMainSpawnWFS;
            }
        }

        private void CreateMainEnemy()
        {
            CalculatestartPositionFirstEnemy();

            _currentMainEnemy.transform.position = new Vector3(_currentMainEnemyPositionX, _currentMainEnemyPositionY, _currentMainEnemyPositionZ);
        }

        private void CalculatestartPositionFirstEnemy()
        {
            bool isWork = true;
            float currentRadiusSpawn;

            while (isWork)
            {
                _currentMainEnemyPositionY = _currentMainEnemy.gameObject.transform.position.y;

                _currentMainEnemyPositionX = Random.Range(_playerMainBilding.gameObject.transform.position.x - _maxRadiusSpawn, _playerMainBilding.gameObject.transform.position.x + _maxRadiusSpawn);
                _currentMainEnemyPositionZ = Random.Range(_playerMainBilding.gameObject.transform.position.z - _maxRadiusSpawn, _playerMainBilding.gameObject.transform.position.z + _maxRadiusSpawn);

                currentRadiusSpawn = Mathf.Sqrt(Mathf.Pow(_currentMainEnemyPositionX - _playerMainBilding.gameObject.transform.position.x, 2) + Mathf.Pow(_currentMainEnemyPositionZ - _playerMainBilding.gameObject.transform.position.z, 2));

                if (currentRadiusSpawn >= _minRadiusSpawn & currentRadiusSpawn <= _maxRadiusSpawn)
                {
                    isWork = false;
                }
            }
        }

        public void CreateMinorEnemies()
        {
            for (int i = 0; i < _minorEnemyCount; i++)
            {
                _currentMinorEnemy = _enemyCollection.GetRandomEnemy();

                if (_currentMinorEnemy != null)
                {
                    CalculateMinorStartPosition();
                    SetMinorStartPosition();
                }
            }
        }

        private void CalculateMinorStartPosition()
        {
            bool isWork = true;
            float radius;

            _currentMinorEnemyPositionY = _currentMainEnemy.transform.position.y;

            while (isWork)
            {
                _currentMinorEnemyPositionX = _currentMainEnemy.transform.position.x + Random.Range(-_maxMinorSpawnRange, _maxMinorSpawnRange);
                _currentMinorEnemyPositionZ = _currentMainEnemy.transform.position.z + Random.Range(-_maxMinorSpawnRange, _maxMinorSpawnRange);

                radius = Mathf.Sqrt(Mathf.Pow(_currentMinorEnemyPositionX - _currentMainEnemy.transform.position.x, 2) + Mathf.Pow(_currentMinorEnemyPositionZ - _currentMainEnemy.transform.position.z, 2));

                if (radius >= _minMinorSpawnRange & radius <= _maxMinorSpawnRange)
                {
                    isWork = false;
                }
            }           
        }

        private void SetMinorStartPosition()
        {
            _currentMinorEnemy.transform.position = new Vector3(_currentMinorEnemyPositionX, _currentMinorEnemyPositionY, _currentMinorEnemyPositionZ);
        }
    }
}