using Assets.Scripts.PlayerComponents.Weapons.Bows;
using Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Bow : Weapon
    {
        [SerializeField] private float _radius;
        [SerializeField] private Vector3 _shootPoint;
        [SerializeField] private Arrow _arrowPrefab;
        [SerializeField] private Mark _mark;

        private ArrowsPool _pool;
        private Enemy _closestTarget;

        private void Awake()
        {
            _pool = new ArrowsPool(_arrowPrefab);

            _mark.Init();
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

                if (closerstCollider.TryGetComponent<Enemy>(out Enemy target))
                {
                    _closestTarget = target;
                    Mark(target);
                }
            }
            else
            {
                UnMark();
            }
        }

        public override void Attack()
        {
            if (_closestTarget != null && CanAttack == true)
            {
                Arrow arrow = _pool.GetArrow();

                arrow.transform.position = transform.position + new Vector3(0, 2, 0);
                arrow.Fly(_closestTarget.transform);

                base.Attack();
            }
        }

        private void Mark(Enemy enemy)
        {
            _mark.MarkEnemy(enemy);
        }

        private void UnMark()
        {
            _mark.UnMarkEnemy();
        }
    }
}
