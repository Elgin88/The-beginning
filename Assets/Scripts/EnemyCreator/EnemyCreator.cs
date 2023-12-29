using System.Collections;
using UnityEngine;

namespace Scripts.Enemy
{
    public class EnemyCreator : MonoBehaviour
    {
        [SerializeField] private GameObject _playerMainBilding;
        [SerializeField] private EnemyCollection _enemyCollection;
        [SerializeField] private float _maxMainEnemyRadiusSpawn;
        [SerializeField] private float _delayBetweenMainEnemySpawn;
        [SerializeField] private float _maxMinorEnemySpawnRange;
        [SerializeField] private float _minorEnemyCount;

        private GameObject _currentMainEnemy;
        private GameObject _currentMinorEnemy;
        private float _currentMainEnemyPositionX;
        private float _currentMainEnemyPositionY;
        private float _currentMainEnemyPositionZ;
        private float _currentMinorEnemyPositionX;
        private float _currentMinorEnemyPositionY;
        private float _currentMinorEnemyPositionZ;
        private float _minMainEnemyRadiusSpawn;
        private float _minMinorEnemySpawnRange;
        private WaitForSeconds _delayBetweenMainEnemySpawnWFS;
        private Coroutine _createAllEnemies;

        private void Start()
        {
            _minMainEnemyRadiusSpawn = _maxMainEnemyRadiusSpawn - 0.5f;
            _minMinorEnemySpawnRange = _maxMinorEnemySpawnRange - 0.5f;

            _delayBetweenMainEnemySpawnWFS = new WaitForSeconds(_delayBetweenMainEnemySpawn);
            _createAllEnemies = StartCoroutine(CreateAllEnemies());
        }

        private IEnumerator CreateAllEnemies()
        {
            bool isWork = true;

            while (isWork)
            {
                _currentMainEnemy = _enemyCollection.GetRandomEnemy();

                if (_currentMainEnemy == null)
                {
                    isWork = false;
                    StopCoroutine(_createAllEnemies);

                    yield break;
                }

                CreateMainEnemy();
                CreateMinorEnemies();

                yield return _delayBetweenMainEnemySpawnWFS;
            }
        }

        private void CreateMainEnemy()
        {
            CalculateMainStartPosition();
            SetMainStartPosition();
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

        private void CalculateMainStartPosition()
        {
            bool isWork = true;
            float currentRadiusSpawn;
            
            _currentMainEnemyPositionY = _currentMainEnemy.transform.position.y;            

            while (isWork)
            {
                _currentMainEnemyPositionX = _playerMainBilding.transform.position.x + Random.Range(- _maxMainEnemyRadiusSpawn, _maxMainEnemyRadiusSpawn);
                _currentMainEnemyPositionZ = _playerMainBilding.transform.position.z + Random.Range(-_maxMainEnemyRadiusSpawn, _maxMainEnemyRadiusSpawn);
                
                currentRadiusSpawn = Mathf.Sqrt(Mathf.Pow((_currentMainEnemyPositionX - _playerMainBilding.gameObject.transform.position.x), 2) + Mathf.Pow((_currentMainEnemyPositionZ - _playerMainBilding.gameObject.transform.position.z), 2));

                if (currentRadiusSpawn >= _minMainEnemyRadiusSpawn)
                {
                    if (currentRadiusSpawn <= _maxMainEnemyRadiusSpawn)
                    {
                        isWork = false;
                    }
                }
            }
        }
        
        private void CalculateMinorStartPosition()
        {
            bool isWork = true;
            float radius;

            _currentMinorEnemyPositionY = _currentMinorEnemy.transform.position.y;

            while (isWork)
            {
                _currentMinorEnemyPositionX = _currentMainEnemy.transform.position.x + Random.Range(-_maxMinorEnemySpawnRange, _maxMinorEnemySpawnRange);
                _currentMinorEnemyPositionZ = _currentMainEnemy.transform.position.z + Random.Range(-_maxMinorEnemySpawnRange, _maxMinorEnemySpawnRange);

                radius = Mathf.Sqrt(Mathf.Pow(_currentMinorEnemyPositionX - _currentMainEnemy.transform.position.x, 2) + Mathf.Pow(_currentMinorEnemyPositionZ - _currentMainEnemy.transform.position.z, 2));

                if (radius >= _minMinorEnemySpawnRange & radius <= _maxMinorEnemySpawnRange)
                {
                    isWork = false;
                }
            }
        }

        private void SetMainStartPosition()
        {
            _currentMainEnemy.transform.position = new Vector3(_currentMainEnemyPositionX, _currentMainEnemyPositionY, _currentMainEnemyPositionZ);
        }

        private void SetMinorStartPosition()
        {
            _currentMinorEnemy.transform.position = new Vector3(_currentMinorEnemyPositionX, _currentMinorEnemyPositionY, _currentMinorEnemyPositionZ);
        }
    }
}