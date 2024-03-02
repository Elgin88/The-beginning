using UnityEngine;
using Zenject;

namespace Assets.Scripts.PlayerUnits
{
    internal class UnitsFactory : MonoBehaviour
    {
        [SerializeField] private Melee _meleePrefab;
        [SerializeField] private ParticleSystem _particleSystemPrefab;
        [SerializeField] private LayerMask _enemyLayerMask;

        private SelectedUnitsHandler _handler;
        private UnitsPool _pool;

        private void Start() 
        { 
            _pool = new UnitsPool(_meleePrefab, _particleSystemPrefab , 2, _enemyLayerMask, _handler);
            _handler.Init(_pool.MeleePool);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Unit unit = _pool.GetMelee();
                unit.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
            }
        }

        [Inject]
        private void Construct(SelectedUnitsHandler selectedUnitsHandler)
        {
            _handler = selectedUnitsHandler;
        }
    }
}
