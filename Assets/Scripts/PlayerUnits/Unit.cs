using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal abstract class Unit : Selectable, IDamageable
    {
        private float _health;
        private float _damage;
        private float _speed;

        private Coroutine _move;

        public bool IsPlayerObject => true;

        public Transform Transform => transform;

        public void TakeDamage(float damage)
        {
            Debug.Log("aaay");
        }

        public void Select()
        {
            
        }

        public void Deselect()
        {
            
        }

        public void Move(Vector3 position)
        {
            float scaledMoveSpeed = _speed * Time.fixedDeltaTime;

            if (_move != null)
            {
                StopCoroutine(_move);
            }

            _move = StartCoroutine(Move(position, scaledMoveSpeed));
        }

        private IEnumerator Move(Vector3 position, float moveSpeed)
        {
            while (transform.position !=  position)
            {
                Vector3.MoveTowards(transform.position, position, moveSpeed);

                yield return null;
            }
        }
    }
}
