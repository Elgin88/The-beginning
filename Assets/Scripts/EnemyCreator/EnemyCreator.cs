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

                CreateEnemy();

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void CreateEnemy()
        {
            CalculatePositionXZSpawnPoint();

            _enemy.transform.position = new Vector3(_spawnPoinPositionX, _spawnPoinPositionY, _spawnPoinPositionZ);
        }

        private void CalculatePositionXZSpawnPoint()
        {
            bool isWork = true;

            while (isWork)
            {
                SetSpawnPointPosition();

                float deltaX = _spawnPoinPositionX - _playerMainBildingPositionX;
                float deltaZ = _spawnPoinPositionZ - _playerMainBildingPositionZ;
                float radius = Mathf.Sqrt(Mathf.Pow(deltaX, 2) + Mathf.Pow(deltaZ, 2));

                if (radius >= _minRadiusSpawn & radius <= _maxRadiusSpawn)
                {
                    isWork = false;
                }
            }
        }

        private void SetSpawnPointPosition()
        {
            float mainBildingX = _playerMainBilding.gameObject.transform.position.x;
            float mainBildingZ = _playerMainBilding.gameObject.transform.position.z;

            _spawnPoinPositionY = _enemy.gameObject.transform.position.y;
            _spawnPoinPositionX = Random.Range(mainBildingX - _maxRadiusSpawn, mainBildingX + _maxRadiusSpawn);
            _spawnPoinPositionZ = Random.Range(mainBildingZ - _maxRadiusSpawn, mainBildingZ + _maxRadiusSpawn);
        }
    }
}