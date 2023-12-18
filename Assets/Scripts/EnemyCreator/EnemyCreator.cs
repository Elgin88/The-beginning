using System.Collections;
using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
    [SerializeField] private GameObject _playerMainBilding;
    [SerializeField] private EnemyMelee _enemyMelee;
    [SerializeField] private float _minRadiusSpawn;
    [SerializeField] private float _maxRadiusSpawn;

    private float _spawnPoinPositionX;
    private float _spawnPoinPositionY = 0;
    private float _spawnPoinPositionZ;

    private void Start()
    {
        StartCoroutine(CreateEnemies());
    }

    private IEnumerator CreateEnemies()
    {
        while (true)
        {
            CreateEnemy();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CreateEnemy()
    {
        CalculatePositionXZSpawnPoint();

        _enemyMelee.transform.position = new Vector3(_spawnPoinPositionX, _spawnPoinPositionY, _spawnPoinPositionZ);
    }

    private void CalculatePositionXZSpawnPoint()
    {
        bool isWork = true;

        while (isWork)
        {
            SetPositionX();
            SetPositionZ();

            float deltaX = _playerMainBilding.transform.position.x - _spawnPoinPositionX;
            float deltaY = _playerMainBilding.transform.position.y - _spawnPoinPositionY;

            if (Mathf.Sqrt(Mathf.Pow(deltaX, 2) + Mathf.Pow(deltaY, 2)) > _minRadiusSpawn)
            {
                isWork = false;
            }
        }
    }

    private void SetPositionX()
    {
        _spawnPoinPositionX = Random.Range(_playerMainBilding.gameObject.transform.position.x - _maxRadiusSpawn, _playerMainBilding.gameObject.transform.position.x + _maxRadiusSpawn);
    }

    private void SetPositionZ()
    {
        _spawnPoinPositionZ = Random.Range(_playerMainBilding.gameObject.transform.position.z - _maxRadiusSpawn, _playerMainBilding.gameObject.transform.position.z + _maxRadiusSpawn);
    }
}