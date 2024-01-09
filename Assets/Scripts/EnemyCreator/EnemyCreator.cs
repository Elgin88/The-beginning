using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyCreator : MonoBehaviour
    {
        [SerializeField] private float _delayBetweenEnemySpawn;
        [SerializeField] private float _minorEnemySpawnRangeMax;
        [SerializeField] private float _minorEnemyCount;

        private WaitForSeconds _delayBetweenMainEnemySpawnWFS;
        private SpawnPoint[] _spawnPoints;
        private SpawnPoint _currentSpawnPoint;
        private GameObject _currentMainEnemy;
        private GameObject _currentMinorEnemy;
        private Coroutine _createAllEnemies;
        private float _currentMinorEnemyPositionX;
        private float _currentMinorEnemyPositionY;
        private float _currentMinorEnemyPositionZ;
        private float _minorEnemySpawnRangeMin;

        private void Start()
        {
            _spawnPoints = GetComponentsInChildren<SpawnPoint>();
            _delayBetweenMainEnemySpawnWFS = new WaitForSeconds(_delayBetweenEnemySpawn);

            _minorEnemySpawnRangeMin = _minorEnemySpawnRangeMax - 0.5f;

            _createAllEnemies = StartCoroutine(CreateAllEnemies());
        }

        private IEnumerator CreateAllEnemies()
        {
            bool isWork = true;

            while (isWork)
            {
                yield return null;

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

        private void CreateMinorEnemies()
        {
            for (int i = 0; i < _minorEnemyCount; i++)
            {
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
                _currentMinorEnemyPositionX = _currentMainEnemy.transform.position.x + Random.Range(-_minorEnemySpawnRangeMax, _minorEnemySpawnRangeMax);
                _currentMinorEnemyPositionZ = _currentMainEnemy.transform.position.z + Random.Range(-_minorEnemySpawnRangeMax, _minorEnemySpawnRangeMax);

                radius = Mathf.Sqrt(Mathf.Pow(_currentMinorEnemyPositionX - _currentMainEnemy.transform.position.x, 2) + Mathf.Pow(_currentMinorEnemyPositionZ - _currentMainEnemy.transform.position.z, 2));

                if (radius >= _minorEnemySpawnRangeMin & radius <= _minorEnemySpawnRangeMax)
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