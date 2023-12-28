using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Arrow : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _hitEffect;

        private void OnTriggerEnter(Collider other)
        {
            ParticleSystem hitEffect = Instantiate(_hitEffect);
            Destroy(hitEffect, 1.5f);
            gameObject.SetActive(false);
        }
    }
}