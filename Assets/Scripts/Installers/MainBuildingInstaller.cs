using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;
using Zenject;

public class PlayerMainBildingInstaller : MonoInstaller
{
    [SerializeField] private MainBuilding _playerMainBiulding;

    public override void InstallBindings()
    {
        MainBuilding playerMainBuilding = Container.InstantiatePrefabForComponent<MainBuilding>(_playerMainBiulding, _playerMainBiulding.transform.position, Quaternion.identity, null);

        Container.Bind<MainBuilding>().FromInstance(_playerMainBiulding).AsSingle().NonLazy();
    }
}