using System.Collections;
using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    internal class EnemyFactory: MonoBehaviour
    {
        [SerializeField] private EnemyMeleeOgreGreen _enemyMeleeOrc;
        //[SerializeField] private EnemyRangeWoodArcher _enemyRangeWoodArcher;
        [SerializeField] private float _delayBetweenWavesEnemy;
        [SerializeField] private float _rangeMinorSpawn;
        [SerializeField] private float _countMinorEnemy;

        [Inject] private DiContainer _currentEnemyDI;

        private MainBuilding _mainBuilding; 
        
        private EnemySpawnPoint[] _spawnPoints;
        private EnemySpawnPoint _currentSpawnPoint;
        private WaitForSeconds _delayBetweenWavesEnemyWFS;
        private Coroutine _createAllEnemies;
        private Vector3 _currentEnemyPosition;
        private float _minorEnemySpawnRangeMin => _rangeMinorSpawn - 0.5f;

        private void Awake()
        {
            _mainBuilding = FindObjectOfType<MainBuilding>();

            _spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();

            _delayBetweenWavesEnemyWFS = new WaitForSeconds(_delayBetweenWavesEnemy);

            _createAllEnemies = StartCoroutine(CreateAllEnemies());
        }

        private IEnumerator CreateAllEnemies()
        {
            bool isWork = true;

            while (isWork)
            {
                ChooseSpawnPoint();
                CreateMainEnemyInWave();
                CreateMinorEnemiesInWave();

                yield return _delayBetweenWavesEnemyWFS;
            }

            StopCoroutine(_createAllEnemies);
        }

        private void CreateMainEnemyInWave()
        {
            SpawnRandomEnemy();
        }

        private void CreateMinorEnemiesInWave()
        {
            for (int i = 0; i <= _countMinorEnemy; i++)
            {
                CalculateMinorUnitStartPosition();
                SpawnRandomEnemy();
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
                _currentEnemyPosition.x = _currentSpawnPoint.transform.position.x + Random.Range(-_rangeMinorSpawn, _rangeMinorSpawn);
                _currentEnemyPosition.y = gameObject.transform.position.y;
                _currentEnemyPosition.z = _currentSpawnPoint.transform.position.z + Random.Range(-_rangeMinorSpawn, _rangeMinorSpawn);

                radius = Mathf.Sqrt(Mathf.Pow(_currentEnemyPosition.x - _currentSpawnPoint.transform.position.x, 2) + Mathf.Pow(_currentEnemyPosition.z - _currentSpawnPoint.transform.position.z, 2));

                if (radius >= _minorEnemySpawnRangeMin & radius <= _rangeMinorSpawn)
                {
                    isWork = false;
                }
            }
        }

        private void SpawnRandomEnemy()
        {
            switch (Random.Range(1, 3))
            {
                case 1:
                    SpawnEnemy(_enemyMeleeOrc.gameObject);
                    break;

                case 2:
                    SpawnEnemy(_enemyMeleeOrc.gameObject);
                    //SpawnEnemy(_enemyRangeWoodArcher.gameObject);
                    break;
            }
        }

        private void SpawnEnemy(GameObject gameObject)
        {
            if (_currentEnemyPosition != new Vector3(0,0,0) & _mainBuilding != null)
            {
                _currentEnemyDI.InstantiatePrefab(gameObject, _currentEnemyPosition, Quaternion.LookRotation(_mainBuilding.transform.position), null);
            }
        }
    }
}