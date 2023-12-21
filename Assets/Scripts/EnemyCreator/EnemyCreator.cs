using System.Collections;
using UnityEngine;

namespace Scripts.Enemy
{
    public class EnemyCreator : MonoBehaviour
    {
        [SerializeField] private GameObject _playerMainBilding;
        [SerializeField] private float _minRadiusSpawn;
        [SerializeField] private float _maxRadiusSpawn;
        [SerializeField] private BankOfEnemies _bankOfEnemies;

        private float _spawnPoinPositionX;
        private float _spawnPoinPositionY;
        private float _spawnPoinPositionZ;
        private float _playerMainBildingPositionX => _playerMainBilding.gameObject.transform.position.x;
        private float _playerMainBildingPositionZ => _playerMainBilding.gameObject.transform.position.z;
        private float _currentRadiusSpawn;

        private GameObject _enemy;

        private void Start()
        {
            StartCoroutine(CreateEnemies());
        }

        private IEnumerator CreateEnemies()
        {
            while (true)
            {
                _enemy = _bankOfEnemies.GetMeleeEnemy();

                if (_enemy == null)
                {
                    StopCoroutine(CreateEnemies());
                    yield break;
                }

                CreateFirstEnemy();

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void CreateFirstEnemy()
        {
            CalculatePositionSpawnPointFirstEnemy();

            _enemy.transform.position = new Vector3(_spawnPoinPositionX, _spawnPoinPositionY, _spawnPoinPositionZ);
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
            _spawnPoinPositionY = _enemy.gameObject.transform.position.y;
            _spawnPoinPositionX = Random.Range(_playerMainBildingPositionX - _maxRadiusSpawn, _playerMainBildingPositionX + _maxRadiusSpawn);
            _spawnPoinPositionZ = Random.Range(_playerMainBildingPositionZ - _maxRadiusSpawn, _playerMainBildingPositionZ + _maxRadiusSpawn);
        }
    }
}