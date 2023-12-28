using Scripts.Enemy;
using System;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class Bow : Weapon
    {
        [SerializeField] private float _radius;
        [SerializeField] private Vector3 _shootPoint;
        [SerializeField] private Arrow _arrowPrefab;
        [SerializeField] private Mark _markPrefab;

        private Enemy _closestTarget;

        private void Awake()
        {
            _markPrefab.gameObject.SetActive(false);
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

                if (closerstCollider.TryGetComponent<Enemy>(out Enemy target))
                {
                    _closestTarget = target;
                    Mark(target);
                }
            }

            UnMark();
        }

        public override void Attack()
        {
            if (_closestTarget != null)
            {

            }
        }

        private void Mark(Enemy enemy)
        {
            _markPrefab.MarkEnemy(enemy);
        }

        private void UnMark()
        {
            _markPrefab.UnMarkEnemy();
        }
    }
}
