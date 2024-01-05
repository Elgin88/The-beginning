using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

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

        public void MarkEnemy(IDamageable enemy)
        {
            _mark.transform.position = enemy.Transform.position;

            if (_mark.isStopped)
                _mark.Play();
        }

        public void UnMarkEnemy()
        {
            _mark.Stop();
        }
    }
}