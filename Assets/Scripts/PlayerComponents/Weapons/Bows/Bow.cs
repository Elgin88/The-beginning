using System.Collections;
using UnityEngine;
using Assets.Scripts.PlayerComponents.Weapons.Bows;
using Assets.Scripts.AnimatorScripts.Player;
using Assets.Scripts.EnemyComponents;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Bow : Weapon
    {
        [SerializeField] private float _radius;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Arrow _arrowPrefab;
        [SerializeField] private Mark _mark;
        [SerializeField] private PlayerBowAttack _attack;
        [SerializeField] private LayerMask _layerMask;

        private Coroutine _attackCoroutine;
        private ArrowsPool _pool;
        private IEnemy _closestTarget;

        private void Awake()
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
                Collider closerstCollider = hitColliders[0];

                for (int i = 0; i < hitColliders.Length; i++)
                {
                    distance = Vector3.Distance(transform.position, hitColliders[i].gameObject.transform.position);

                    if (distance <= closestDistance)
                    {
                        closestDistance = distance;
                        closerstCollider = hitColliders[i];
                    }
                }

                if (closerstCollider.TryGetComponent<IEnemy>(out IEnemy target))
                {
                    _closestTarget = target;
                    Mark(target);
                }
            }
            else
            {
                _closestTarget = null;
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
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(AttackDelay(AttackSpeed));
        }

        private IEnumerator AttackDelay(float attackSpeed)
        {
            base.Attack();

            yield return new WaitForSeconds(attackSpeed - 0.65f);

            if (_closestTarget != null)
            {
                Arrow arrow = _pool.GetArrow();

                arrow.transform.position = _shootPoint.position;
                arrow.Fly(_closestTarget.Position);
            }
        }

        private void Mark(IEnemy enemy)
        {
            _mark.MarkEnemy(enemy);
        }

        private void UnMark()
        {
            _mark.UnMarkEnemy();
        }
    }
}
