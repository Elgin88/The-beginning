using Assets.Scripts.GameLogic;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents.Weapons;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal abstract class Unit : Selectable, IDamageable, IMoveable
    {
        private float _radius;
        private float _health;
        private float _speed;

        private LayerMask _layerMask;
        private ClosestTargetFinder _closestTargetFinder;
        public bool IsDead => _isDead;

        private IDamageable _closestTarget;
        private Coroutine _move;

        public bool IsPlayerObject => true;

        public Transform Transform => transform;

        public float MoveSpeed => _speed;

        public SurfaceAlignment SurfaceAlignment => new SurfaceAlignment(this);

        private void FixedUpdate()
        {
            if (_closestTarget != null && _closestTarget.Transform.gameObject.activeSelf)
                return;

            _closestTarget = _closestTargetFinder.FindTarget(transform.position);
        }

        public void TakeDamage(float damage)
        {
            if (_health <= 0)
                Die();
        }

        public void InitUnit(float health, float speed)
        {
            _health = health;
            _speed = speed;

            _closestTargetFinder = new ClosestTargetFinder(_radius, _layerMask);
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
            while (transform.position != position)
            {
                transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed);
                Vector3 movementVector = transform.position - position;

                if (Physics.Raycast(transform.position, position, out RaycastHit hit, 1f, _layerMask))
                {
                    if (hit.collider.TryGetComponent<IDamageable>(out IDamageable unit))
                    {
                        position = transform.position;
                    }
                }

                movementVector = new Vector3(movementVector.x, 0, movementVector.y);
                SurfaceAlignment.Align(movementVector);

                yield return null;
            }
        }

        private void Attack(IDamageable target)
        {
            
        }

        private void Die()
        {
            gameObject.SetActive(false);
        }
    }
}