using Assets.Scripts.BuildingSystem.Buildings;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    internal class EnemyFactory: MonoBehaviour
    {
        [SerializeField] private EnemyMeleeOgreGreen _enemyMeleeOrc;
        [SerializeField] private EnemyRange _rangeEneny;
        [SerializeField] private float _delayBetweenWavesEnemy;
        [SerializeField] private float _minorEnemySpawnRangeMax;
        [SerializeField] private float _minorEnemyCount;

        [Inject] private DiContainer _currentEnemyDI;
        [Inject] private MainBuilding _mainBuilding; 
        
        private EnemySpawnPoint[] _spawnPoints;
        private EnemySpawnPoint _currentSpawnPoint;
        private WaitForSeconds _delayBetweenWavesEnemyWFS;
        private Coroutine _createAllEnemies;
        private Vector3 _currentEnemyPosition;
        private float _minorEnemySpawnRangeMin => _minorEnemySpawnRangeMax - 0.5f;

        private void Awake()
        {
            _spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();

            _delayBetweenWavesEnemyWFS = new WaitForSeconds(_delayBetweenWavesEnemy);

            _createAllEnemies = StartCoroutine(CreateAllEnemies());
        }

        private IEnumerator CreateAllEnemies()
        {
            bool isWork = true;

            while (isWork)
            {
                CreateMainEnemyInWave();
                CreateMinorEnemiesInWave();

                yield return _delayBetweenWavesEnemyWFS;
            }

            StopCoroutine(_createAllEnemies);
        }

        private void CreateMainEnemyInWave()
        {
            ChooseSpawnPoint();
            SpawnRandom();
        }

        private void CreateMinorEnemiesInWave()
        {
            for (int i = 0; i < _minorEnemyCount; i++)
            {
                CalculateMinorUnitStartPosition();
                SpawnRandom();
            }
        }

        private void ChooseSpawnPoint()
        {
            int index = Random.Range(0, _spawnPoints.Length);

            _currentSpawnPoint = _spawnPoints[index];
        }

        private void CalculateMinorUnitStartPosition()
        {
            bool isWork = true;
            float radius;

            while (isWork)
            {
                _currentEnemyPosition.x = _currentSpawnPoint.transform.position.x + Random.Range(-_minorEnemySpawnRangeMax, _minorEnemySpawnRangeMax);
                _currentEnemyPosition.y = gameObject.transform.position.y;
                _currentEnemyPosition.z = _currentSpawnPoint.transform.position.z + Random.Range(-_minorEnemySpawnRangeMax, _minorEnemySpawnRangeMax);

                radius = Mathf.Sqrt(Mathf.Pow(_currentEnemyPosition.x - _currentSpawnPoint.transform.position.x, 2) + Mathf.Pow(_currentEnemyPosition.z - _currentSpawnPoint.transform.position.z, 2));

                if (radius >= _minorEnemySpawnRangeMin & radius <= _minorEnemySpawnRangeMax)
                {
                    isWork = false;
                }
            }
        }

        private void SpawnRandom()
        {
            switch (Random.Range(1, 3))
            {
                case 1:
                    SpawnMeleeEnemy();
                    break;

                case 2:
                    SpawnRangeEnemy();
                    break;

                default:
                    break;
            }
        }

        private void SpawnMeleeEnemy()
        {
            _currentEnemyDI.InstantiatePrefab(_enemyMeleeOrc, _currentEnemyPosition, Quaternion.LookRotation(_mainBuilding.transform.position), null);
        }

        private void SpawnRangeEnemy()
        {
            _currentEnemyDI.InstantiatePrefab(_rangeEneny, _currentEnemyPosition, Quaternion.identity, null);
        }
    }
}