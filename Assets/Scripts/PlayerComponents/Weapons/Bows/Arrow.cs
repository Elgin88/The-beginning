using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Arrow : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _hitEffect;
        [SerializeField] private float _speed;

        private Coroutine _flying;

        private void OnTriggerEnter(Collider other)
        {
            ParticleSystem hitEffect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
            Destroy(hitEffect.gameObject, 1f);
            gameObject.SetActive(false);
        }

        public void Fly(Transform target)
        {
            if (_flying != null)
            {
                StopCoroutine(_flying);
            }
                
            _flying = StartCoroutine(Flying(target));
        }

        private IEnumerator Flying(Transform target)
        {
            while(Vector3.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);

                Vector3 relativePosition = target.position - transform.position;
                transform.rotation = Quaternion.LookRotation(relativePosition, Vector3.up);

                yield return null;
            }
        }
    }
}