using System.Collections;
using UnityEngine;
using Assets.Scripts.Enemy;

namespace Assets.Scripts.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    internal class MoveableCollider : MonoBehaviour
    {
        [SerializeField] private float _force;
        [SerializeField] private float _stopSpeed;

        private Rigidbody _rigidbody;
        private Coroutine _stop;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent<IMoveable>(out IMoveable moveable))
            {
                if (this.gameObject.TryGetComponent<EnemyMelee>(out EnemyMelee enemy))
                    return;

                Vector3 direction = collision.contacts[0].point - transform.position;
                direction = -direction.normalized;

                _rigidbody.AddForce(direction * _force, ForceMode.Impulse);
            }

            if (_stop != null)
            {
                StopCoroutine(_stop);
            }

            _stop = StartCoroutine(Stop(_stopSpeed));
        }

        private IEnumerator Stop(float speed)
        {
            while (_rigidbody.velocity != Vector3.zero)
            {
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, speed * Time.fixedDeltaTime);

                yield return null;
            }
        }
    }
}
