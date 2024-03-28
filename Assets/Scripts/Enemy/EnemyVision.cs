using System.Collections;
using UnityEngine;

namespace Assets.Scripts.EnemyNamespace
{
    internal class EnemyVision: MonoBehaviour
    {
        [SerializeField] private LayerMask _layersForEnemyVision;
        [SerializeField] private float _range;
        [SerializeField] private float _delayVision;

        private WaitForSeconds _delayVisionWFS;
        private Collider _nearestTargetCollider;

        internal Collider NearestTargetCollider => _nearestTargetCollider;

        internal void StartVision()
        {
            StartCoroutine(Vision());
        }

        private void Awake()
        {
            _delayVisionWFS = new WaitForSeconds(_delayVision);
        }

        private IEnumerator Vision()
        {
            Collider[] targetsColliders = new Collider[0];

            while (targetsColliders.Length == 0)
            {
                targetsColliders = Physics.OverlapSphere(transform.position, _range, _layersForEnemyVision);

                SetNearestTrgetCollider(targetsColliders);

                yield return _delayVisionWFS;
            }
        }

        private void SetNearestTrgetCollider(Collider[] targetsColliders)
        {
            float nearestDistance = 100;
            float currentDistance;

            foreach (Collider collider in targetsColliders)
            {
                currentDistance = Vector3.Distance(transform.position, collider.transform.position);

                if (currentDistance < nearestDistance)
                {
                    nearestDistance = currentDistance;
                    _nearestTargetCollider = collider;
                }
            }
        }
    }
}