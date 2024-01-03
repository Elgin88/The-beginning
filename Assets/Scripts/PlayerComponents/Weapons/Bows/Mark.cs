using Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Mark : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particlePrefab;

        private ParticleSystem _mark;

        public void Init()
        {
            _mark = Instantiate(_particlePrefab, transform.position, Quaternion.identity);
            _mark.Stop();
        }

        public void MarkEnemy(Enemy enemy)
        {
            _mark.transform.position = enemy.transform.position;

            if (_mark.isStopped)
                _mark.Play();
        }

        public void UnMarkEnemy()
        {
            _mark.Stop();
        }
    }
}