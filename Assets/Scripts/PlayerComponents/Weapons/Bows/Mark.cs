using Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Mark : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        public void MarkEnemy(Enemy enemy)
        {
            transform.position = enemy.transform.position;
            _particleSystem.Play();
        }

        public void UnMarkEnemy()
        {
            _particleSystem.Pause();
        }
    }
}