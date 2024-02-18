using System.Collections;
using UnityEngine;
using Assets.Scripts.PlayerComponents.Weapons.Bows;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Bow : Weapon
    {
        [SerializeField] private float _radius;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Arrow _arrowPrefab;
        [SerializeField] private Mark _mark;

        private Coroutine _attackCoroutine;
        private ArrowsPool _pool;
        private IDamageable _closestTarget;

        private void Start()
        {
            _pool = new ArrowsPool(_arrowPrefab, Damage);

            _mark.Init();
        }

        private void FixedUpdate()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _radius, _layerMask);

            if (hitColliders.Length > 0)
            {
                float distance;
                float closestDistance = _radius;
                Collider closestCollider = hitColliders[0];

                for (int i = 0; i < hitColliders.Length; i++)
                {
                    distance = Vector3.Distance(transform.position, hitColliders[i].gameObject.transform.position);

                    if (distance <= closestDistance)
                    {
                        closestDistance = distance;
                        closestCollider = hitColliders[i];
                    }
                }

                if (closestCollider.TryGetComponent<IDamageable>(out IDamageable target))
                {
                    _closestTarget = target;
                    Mark(target);
                    CanAttack = true;
                }
            }
            else
            {
                CanAttack = false;
                UnMark();
            }
        }

        private void OnDisable()
        {
            if (_mark != null)
            {
                _mark.UnMarkEnemy();
            }
        }

        public override void Attack()
        {
            base.Attack();

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(AttackDelay(AttackSpeed));
        }

        private IEnumerator AttackDelay(float attackSpeed)
        {
            yield return new WaitForSeconds(attackSpeed - 0.5f);

            Arrow arrow = _pool.GetArrow();

            arrow.transform.position = _shootPoint.position;
            arrow.Fly(_closestTarget.Transform);
        }

        private void Mark(IDamageable enemy)
        {
            _mark.MarkEnemy(enemy);
        }

        private void UnMark()
        {
            _mark.UnMarkEnemy();
        }
    }
}
