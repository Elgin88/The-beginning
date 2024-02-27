using System.Collections;
using UnityEngine;
using Assets.Scripts.PlayerComponents.Weapons.Bows;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.GameLogic;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class PlayerBow : Weapon
    {
        [SerializeField] private Arrow _arrowPrefab;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Mark _mark;
        [SerializeField] private float _radius;

        private float _animationOffset = 0.5f;
        private bool _isOnCooldown = false;

        private Coroutine _attackCoroutine;
        private ArrowsPool _pool;
        private ClosestTargetFinder _closestTargetFinder;
        private IDamageable _closestTarget;

        private void Start()
        {
            _closestTargetFinder = new ClosestTargetFinder(_radius, _layerMask);
            _pool = new ArrowsPool(_arrowPrefab, Damage, _layerMask);

            _mark.Init();
        }

        private void FixedUpdate()
        {
            if (_closestTargetFinder.TryFindTarget(transform.position, out _closestTarget))
            {
                Mark(_closestTarget);
                CanAttack = !_isOnCooldown;
            }
            else
            {
                UnMark();
                CanAttack = false;
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
            _isOnCooldown = true;

            base.Attack();

            yield return new WaitForSeconds(attackSpeed - _animationOffset);

            Arrow arrow = _pool.GetArrow();

            arrow.transform.position = _shootPoint.position;
            arrow.Fly(_closestTarget.Transform);

            yield return new WaitForSeconds(_animationOffset);

            _isOnCooldown = false;
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