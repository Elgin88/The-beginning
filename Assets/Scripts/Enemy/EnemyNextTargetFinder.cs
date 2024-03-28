using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.EnemyNamespace
{
    internal class EnemyNextTargetFinder : MonoBehaviour
    {
        [SerializeField] private EnemyVision _enemyVision;

        [Inject] private MainBuilding _mainBuilding;

        private GameObject _currentTarget;

        internal GameObject CurrentTarget => _currentTarget;

        private void Awake()
        {
            if (_mainBuilding != null)
            {
                _currentTarget = _mainBuilding.gameObject;
            }
        }

        private void Update()
        {
            SetCurrentTarget();
        }

        private void SetCurrentTarget()
        {
            if (_enemyVision.NearestTargetCollider != null)
            {
                _currentTarget = _enemyVision.NearestTargetCollider.gameObject;

                return;
            }

            if (_enemyVision.NearestTargetCollider == null & _mainBuilding != null)
            {
                _currentTarget = _mainBuilding.gameObject;
            }

            _currentTarget = null;

            _enemyVision.StartVision();
        }
    }
}