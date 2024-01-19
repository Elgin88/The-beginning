using UnityEngine;
using Zenject;

public class PlayerMainBildingInstaller : MonoInstaller
{
    [SerializeField] private MainBuilding _playerMainBilding;

    public override void InstallBindings()
    {
        //MainBuilding playerMainBilding = Container.InstantiatePrefabForComponent<MainBuilding>(_playerMainBilding, _playerMainBilding.transform.position, Quaternion.identity, null);

        Container.Bind<MainBuilding>().FromInstance(_playerMainBilding).AsSingle().NonLazy();
    }
}