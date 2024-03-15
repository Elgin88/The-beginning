using Assets.Scripts.BuildingSystem;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents;
using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.PlayerUnits
{
    internal class UnitsFactory : MonoBehaviour
    {
        [SerializeField] private Melee _meleePrefab;
        [SerializeField] private ParticleSystem _particleSystemPrefab;
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private Transform _spotOfRespawnUnits;
        [SerializeField] private SelectedUnitsHandler _handler;

        private UnitsPool _pool;
        private int _spawnUnitButtonIndex = 2;

        public static Action<int> PlayerWentIn;   //также передавать деньги игрока
        public static Action<int> PlayerWentOut;

        private void Start() 
        { 
            _pool = new UnitsPool(_meleePrefab, _particleSystemPrefab , 2, _enemyLayerMask, _handler);
            //_handler.Init(_pool.MeleePool);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Unit unit = _pool.GetMelee();
                unit.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
            }
        }

        private void OnEnable()
        {
            BuildingUI.SpawnButtonClicked += SpawnUnit;
        }

        private void OnDisable()
        {
            BuildingUI.SpawnButtonClicked -= SpawnUnit;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Player player)) // деньги для проверки взять тут
            {
                PlayerWentIn?.Invoke(_spawnUnitButtonIndex);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Player player))
            {
                PlayerWentOut?.Invoke(_spawnUnitButtonIndex);
            }
        }

        private void SpawnUnit()  // сделать проверку на деньги
        {
            Unit unit = _pool.GetMelee();
            unit.transform.position = _spotOfRespawnUnits.transform.position; 
        }
    }
}
