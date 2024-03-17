using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerHealth : MonoBehaviour, IDamageable
    {
        private float _value;
        private float _recoverTime;
        private bool _isDead;

        private Collider _playerCollider;
        private Coroutine _damageRecover;

        public float Value => _value;

        public bool IsPlayerObject => true;

        public bool IsDead => _isDead;

        public Transform Transform => transform;

        private void Start()
        {
            _playerCollider = GetComponent<Collider>();

            _isDead = false;
        }

        public void TakeDamage(float damage)
        {
            _value -= damage;

            if (_value <= 0)
            {
                _isDead = true;
                gameObject.SetActive(false);
            }
            else
            {

                if (_damageRecover != null)
                {
                    StopCoroutine(_damageRecover);
                }

                _damageRecover = StartCoroutine(DamageRecover(_recoverTime));
            }
        }

        private IEnumerator DamageRecover(float time)
        {
            _playerCollider.enabled = false;

            yield return new WaitForSeconds(time);

            _playerCollider.enabled = true;
        }

        [Inject]
        private void Construct(PlayerData playerConfig)
        {
            _value = playerConfig.Health;
            _recoverTime = playerConfig.RecoverTime;
        }
    }
}
