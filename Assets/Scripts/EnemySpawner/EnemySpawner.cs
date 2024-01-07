using System.Collections;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    internal class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float _delayBetweenWavesEnemy;
        [SerializeField] private float _minorEnemySpawnRangeMax;
        [SerializeField] private float _minorEnemyCount;

        [Inject] private EnemyFactory _enemyFactory;

        private WaitForSeconds _delayBetweenWavesEnemyWFS;
        private SpawnPoint[] _spawnPoints;
        private SpawnPoint _currentSpawnPoint;
        private Coroutine _createAllEnemies;
        private Vector3 _minorUnitStartPosition;
        private float _currentEnemyPositionX;
        private float _currentEnemyPositionY;
        private float _currentEnemyPositionZ;
        private float _minorEnemySpawnRangeMin;

        private void Start()
        {
            _spawnPoints = GetComponentsInChildren<SpawnPoint>();
            _delayBetweenWavesEnemyWFS = new WaitForSeconds(_delayBetweenWavesEnemy);

            _minorEnemySpawnRangeMin = _minorEnemySpawnRangeMax - 0.5f;

            _createAllEnemies = StartCoroutine(CreateAllEnemies());
        }

        private IEnumerator CreateAllEnemies()
        {
            bool isWork = true;

            while (isWork)
            {
                CreateMainEnemy();
                CreateMinorEnemies();

                yield return _delayBetweenWavesEnemyWFS;
            }

            StopCoroutine(_createAllEnemies);
        }

        private void CreateMainEnemy()
        {
            ChooseSpawnPoint();            
            _enemyFactory.SpawnRandom(_currentSpawnPoint.transform.position);
        }

        private void CreateMinorEnemies()
        {
            for (int i = 0; i < _minorEnemyCount; i++)
            {
                CalculateMinorUnitStartPosition();
                _enemyFactory.SpawnRandom(_minorUnitStartPosition);
            }
        }

        private void ChooseSpawnPoint()
        {
            int index = Random.Range(0, _spawnPoints.Length - 1);

            _currentSpawnPoint = _spawnPoints[index];
        }
        
        private void CalculateMinorUnitStartPosition()
        {
            bool isWork = true;
            float radius;

            while (isWork)
            {
                _currentEnemyPositionX = _currentSpawnPoint.transform.position.x + Random.Range(-_minorEnemySpawnRangeMax, _minorEnemySpawnRangeMax);
                _currentEnemyPositionY = gameObject.transform.position.y;
                _currentEnemyPositionZ = _currentSpawnPoint.transform.position.z + Random.Range(-_minorEnemySpawnRangeMax, _minorEnemySpawnRangeMax);

                radius = Mathf.Sqrt(Mathf.Pow(_currentEnemyPositionX - _currentSpawnPoint.transform.position.x, 2) + Mathf.Pow(_currentEnemyPositionZ - _currentSpawnPoint.transform.position.z, 2));

                if (radius >= _minorEnemySpawnRangeMin & radius <= _minorEnemySpawnRangeMax)
                {
                    isWork = false;

                    _minorUnitStartPosition = new Vector3(_currentEnemyPositionX, _currentEnemyPositionY, _currentEnemyPositionZ);
                }
            }
        } 
    }
}