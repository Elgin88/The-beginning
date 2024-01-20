using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents.Weapons.Bows;
using Assets.Scripts.AnimatorScripts.Player;
using System.Collections;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Bow : Weapon
    {
        [SerializeField] private float _radius;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Arrow _arrowPrefab;
        [SerializeField] private Mark _mark;
        [SerializeField] private PlayerBowAttack _attack;

        private bool _isAnimating;
        private Coroutine _attackCoroutine;
        private ArrowsPool _pool;
        private IDamageable _closestTarget;

        private void Awake()
        {
            _pool = new ArrowsPool(_arrowPrefab);

            _mark.Init();
        }

        private void OnEnable()
        {
            _attack.AnimationFinished += IsAnimating;
        }

        private void FixedUpdate()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _radius, LayerMask);

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

                if (closerstCollider.TryGetComponent<IDamageable>(out IDamageable target))
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

            _attack.AnimationFinished -= IsAnimating;
        }

        public override void Attack()
        {
            if (_closestTarget != null)
            {
                Arrow arrow = _pool.GetArrow();

                arrow.transform.position = _shootPoint.position;
                arrow.Fly(_closestTarget.Transform);

                base.Attack();
            }
        }

        private IEnumerator AttackCorouite()
        {
            while (_isAnimating)
            {
                yield return null;
            }
        }

        private void IsAnimating(bool isAnimating)
        {
            _isAnimating = isAnimating;
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
