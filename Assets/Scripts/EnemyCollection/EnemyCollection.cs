using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyCollection : MonoBehaviour
    {
        [SerializeField] private MeleeEnemies _meleeEnemies;
        [SerializeField] private RangeEnemies _rangeEnemies;

        private GameObject _enemy;

        public GameObject GetRandomEnemy()
        {
            int numberOfArray = Random.Range(1,3);

            switch (numberOfArray)
            {
                case 1:

                    _enemy = _meleeEnemies.GetEnemy();

                    if (_enemy == null)
                    {
                        _enemy = _rangeEnemies.GetEnemy();
                    }

                    return _enemy;

                case 2:

                    _enemy = _rangeEnemies.GetEnemy();

                    if (_enemy == null)
                    {
                        _enemy = _meleeEnemies.GetEnemy();
                    }

                    return _enemy;

                default:
                    break;
            }

            return null;
        }
    }
}