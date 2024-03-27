using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;

namespace Assets.Scripts.EnemyNamespace
{
    internal class EnemyFactory: MonoBehaviour
    {
        [SerializeField] private MainBuilding _mainBuilding;
        [SerializeField] private float _delayBetweenWaves;
        [SerializeField] private float _maxRangeOfSpawn;
        [SerializeField] private float _enemyCountInWave;
        [SerializeField] private List<EnemyMelleeOrcGreen> _enemiesMeleeOrcs;
        [SerializeField] private List<EnemyRangeWoodArcher> _enemiesMeleeArchers;
        [SerializeField] private EnemySpawnPoint[] _spawnPoints;

        private EnemySpawnPoint _currentSpawnPoint;
        private WaitForSeconds _delayBetweenWavesWFS;
        private Coroutine _createEnemy;
        private Vector3 _enemyPosition;
        private float _minRangeOfSpawn => _maxRangeOfSpawn - 0.5f;

        private void Awake()
        {
            _delayBetweenWavesWFS = new WaitForSeconds(_delayBetweenWaves);
            _createEnemy = StartCoroutine(CreateEnemy());
        }

        private IEnumerator CreateEnemy()
        {
            bool isWork = true;

            while (isWork)
            {
                ChooseSpawnPoint();
                CalculateEnemyPosition();
                EnableRandomEnemy();

                yield return _delayBetweenWavesWFS;
            }

            StopCoroutine(_createEnemy);
        }

        private void ChooseSpawnPoint()
        {
            int index = Random.Range(0, _spawnPoints.Length);

            _currentSpawnPoint = _spawnPoints[index];
        }

        private void CalculateEnemyPosition()
        {
            bool isWork = true;
            float radius;

            while (isWork)
            {
                _enemyPosition.x = _currentSpawnPoint.transform.position.x + Random.Range(-_maxRangeOfSpawn, _maxRangeOfSpawn);
                _enemyPosition.y = gameObject.transform.position.y;
                _enemyPosition.z = _currentSpawnPoint.transform.position.z + Random.Range(-_maxRangeOfSpawn, _maxRangeOfSpawn);

                radius = Mathf.Sqrt(Mathf.Pow(_enemyPosition.x - _currentSpawnPoint.transform.position.x, 2) + Mathf.Pow(_enemyPosition.z - _currentSpawnPoint.transform.position.z, 2));

                if (radius >= _minRangeOfSpawn & radius <= _maxRangeOfSpawn)
                {
                    isWork = false;
                }
            }
        }

        private void EnableRandomEnemy()
        {
            int index = Random.Range(0, 2);

            switch (index)
            {
                case 0:
                    EnableMeleeOrcs(_enemiesMeleeOrcs);
                    break;

                case 1:
                    EnableEnemyRangeArchers(_enemiesMeleeArchers);
                    break;

                default:
                    break;
            }
        }

        private void EnableMeleeOrcs(List<EnemyMelleeOrcGreen> enemies)
        {
            foreach (EnemyMelleeOrcGreen enemy in enemies)
            {
                if (enemy != null)
                {
                    if (enemy.isActiveAndEnabled == false)
                    {
                        enemy.SetPosition(_currentSpawnPoint.transform.position);
                        enemy.gameObject.SetActive(true);

                        return;
                    }
                }
            }
        }

        private void EnableEnemyRangeArchers(List<EnemyRangeWoodArcher> enemiesMeleeArchers)
        {
            foreach (EnemyRangeWoodArcher enemy in enemiesMeleeArchers)
            {
                if (enemy != null)
                {
                    if (enemy.isActiveAndEnabled == false)
                    {
                        enemy.SetPosition(_currentSpawnPoint.transform.position);
                        enemy.gameObject.SetActive(true);

                        return;
                    }
                }
            }
        }


    }
}