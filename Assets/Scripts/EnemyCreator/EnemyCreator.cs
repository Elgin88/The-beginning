using System.Collections;
using UnityEngine;

namespace Scripts.Enemy
{
    public class EnemyCreator : MonoBehaviour
    {
        [SerializeField] private GameObject _playerMainBilding;
        [SerializeField] private float _minRadiusSpawn;
        [SerializeField] private float _maxRadiusSpawn;
        [SerializeField] private EnemyCollection _enemyCollectiron;
        [SerializeField] private EnemyCreatorSpawn _enemyCreatorSpawn;
        [SerializeField] private float _delayBeweenSpawn;

        private float _spawnPoinPositionX;
        private float _spawnPoinPositionY;
        private float _spawnPoinPositionZ;
        private float _playerMainBildingPositionX => _playerMainBilding.gameObject.transform.position.x;
        private float _playerMainBildingPositionZ => _playerMainBilding.gameObject.transform.position.z;
        private float _currentRadiusSpawn;
        private GameObject _mainEnemy;
        private WaitForSeconds _delayWFS;

        private void Start()
        {
            _delayWFS = new WaitForSeconds(_delayBeweenSpawn);

            StartCoroutine(CreateEnemies());
        }

        private IEnumerator CreateEnemies()
        {
            while (true)
            {
                _mainEnemy = _enemyCollectiron.GetRandomEnemy();

                if (_mainEnemy == null)
                {
                    StopCoroutine(CreateEnemies());
                    yield break;
                }

                CreateFirstEnemy();
                _enemyCreatorSpawn.CreateMinorEnemies(_mainEnemy.transform.position);

                yield return _delayWFS;
            }
        }

        private void CreateFirstEnemy()
        {
            CalculatePositionSpawnPointFirstEnemy();

            _mainEnemy.transform.position = new Vector3(_spawnPoinPositionX, _spawnPoinPositionY, _spawnPoinPositionZ);
        }

        private void CalculatePositionSpawnPointFirstEnemy()
        {
            bool isWork = true;

            while (isWork)
            {
                SetSpawnPointPositionFirstEnemy();

                _currentRadiusSpawn = Mathf.Sqrt(Mathf.Pow(_spawnPoinPositionX - _playerMainBildingPositionX, 2) + Mathf.Pow(_spawnPoinPositionZ - _playerMainBildingPositionZ, 2));

                if (_currentRadiusSpawn >= _minRadiusSpawn & _currentRadiusSpawn <= _maxRadiusSpawn)
                {
                    isWork = false;
                }
            }
        }

        private void SetSpawnPointPositionFirstEnemy()
        {
            _spawnPoinPositionY = _mainEnemy.gameObject.transform.position.y;
            _spawnPoinPositionX = Random.Range(_playerMainBildingPositionX - _maxRadiusSpawn, _playerMainBildingPositionX + _maxRadiusSpawn);
            _spawnPoinPositionZ = Random.Range(_playerMainBildingPositionZ - _maxRadiusSpawn, _playerMainBildingPositionZ + _maxRadiusSpawn);
        }
    }
}