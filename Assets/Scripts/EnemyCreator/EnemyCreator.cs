using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyCreator : MonoBehaviour
    {
        [SerializeField] private GameObject _playerMainBilding;
        [SerializeField] private EnemyCollection _enemyCollection;
        [SerializeField] private float _maxMainEnemyRadiusSpawn;
        [SerializeField] private float _delayBetweenMainEnemySpawn;
        [SerializeField] private float _maxMinorEnemySpawnRange;
        [SerializeField] private float _minorEnemyCount;

        private SpawnPoint[] _spawnPoints;
        private SpawnPoint _currentSpawnPoint;
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

            _spawnPoints = GetComponentsInChildren<SpawnPoint>();
        }

        private IEnumerator CreateAllEnemies()
        {
            bool isWork = true;

            while (isWork)
            {
                yield return null;

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
            ChooseSpawnPoint();
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

        private void ChooseSpawnPoint()
        {
            int index = Random.Range(0, _spawnPoints.Length - 1);

            _currentSpawnPoint = _spawnPoints[index];
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
            _currentMainEnemy.transform.position = new Vector3(_currentSpawnPoint.transform.position.x, _currentSpawnPoint.transform.position.y, _currentSpawnPoint.transform.position.z);
        }

        private void SetMinorStartPosition()
        {
            _currentMinorEnemy.transform.position = new Vector3(_currentMinorEnemyPositionX, _currentMinorEnemyPositionY, _currentMinorEnemyPositionZ);
        }
    }
}