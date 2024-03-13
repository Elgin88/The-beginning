using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;
using Zenject;

public class PlayerMainBildingInstaller : MonoInstaller
{
    [SerializeField] private Assets.Scripts.BuildingSystem.Buildings.MainBuilding _playerMainBiulding;

    public override void InstallBindings()
    {
        Assets.Scripts.BuildingSystem.Buildings.MainBuilding playerMainBuilding = Container.InstantiatePrefabForComponent<Assets.Scripts.BuildingSystem.Buildings.MainBuilding>(_playerMainBiulding, _playerMainBiulding.transform.position, Quaternion.identity, null);

        Container.Bind<Assets.Scripts.BuildingSystem.Buildings.MainBuilding>().FromInstance(_playerMainBiulding).AsSingle().NonLazy();
    }
}