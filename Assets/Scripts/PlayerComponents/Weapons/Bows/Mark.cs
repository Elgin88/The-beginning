using UnityEngine;
using Assets.Scripts.EnemyComponents;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Mark : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particlePrefab;

        private ParticleSystem _mark;

        private IEnemy _target;

        public Transform Target => _target.Position;

        public void Init()
        {
            _mark = Instantiate(_particlePrefab, transform.position, Quaternion.identity);
            _mark.Stop();
        }

        public void MarkEnemy(IEnemy enemy)
        {
            _target = enemy;
            _mark.transform.position = _target.Position.position;

            if (_mark.isStopped)
                _mark.Play();
        }

        public void UnMarkEnemy()
        {
            if (_mark != null)
            {
                _mark.Stop();
            }
        }
    }
}