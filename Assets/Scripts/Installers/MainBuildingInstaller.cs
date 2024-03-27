using Assets.Scripts.BuildingSystem.Buildings;
using UnityEngine;
using Zenject;

public class PlayerMainBildingInstaller : MonoInstaller
{
    [SerializeField] private MainBuilding _playerMainBiulding;

    public override void InstallBindings()
    {
        Container.Bind<MainBuilding>().FromInstance(_playerMainBiulding).AsSingle().NonLazy();
    }
}