using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Mark : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particlePrefab;

        private ParticleSystem _markEffect;

        private IDamageable _target;

        public Transform Target => _target.Transform;

        public void Init()
        {
            _markEffect = Instantiate(_particlePrefab, transform.position, Quaternion.identity);
            _markEffect.Stop();
        }

        public void MarkEnemy(IDamageable enemy)
        {
            _target = enemy;
            _markEffect.transform.position = _target.Transform.position;

            if (_markEffect.isStopped)
                _markEffect.Play();
        }

        public void UnMarkEnemy()
        {
            if (_markEffect != null)
            {
                _markEffect.Stop();
            }
        }
    }
}