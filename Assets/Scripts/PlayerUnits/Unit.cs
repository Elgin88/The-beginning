using Assets.Scripts.GameLogic;
using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal abstract class Unit : Selectable, IDamageable, IMoveable
    {
        private float _health;
        private float _damage;
        private float _speed;

        private Coroutine _move;

        public bool IsPlayerObject => true;

        public Transform Transform => transform;

        public float MoveSpeed => 2;

        public float RotationSpeed => 5;

        public SurfaceAlignment SurfaceAlignment => new SurfaceAlignment(this);

        public void TakeDamage(float damage)
        {
            Debug.Log("aaay");
        }

        public void InitStats(float health, float damage, float speed)
        {
            _health = health;
            _damage = damage;
            _speed = speed;
        }

        public void Move(Vector3 position)
        {
            float scaledMoveSpeed = MoveSpeed * Time.fixedDeltaTime;

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
                transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed);

                Vector3 movementVector = transform.position - position;
                movementVector = new Vector3(movementVector.x, 0, movementVector.y);
                SurfaceAlignment.Align(movementVector);

                yield return null;
            }
        }

        private void Die()
        {
            gameObject.SetActive(false);
        }
    }
}
