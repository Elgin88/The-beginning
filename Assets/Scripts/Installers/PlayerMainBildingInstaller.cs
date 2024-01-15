using Assets.Scripts.Bildings;
using UnityEngine;
using Zenject;

public class PlayerMainBildingInstaller : MonoInstaller
{
    [SerializeField] private PlayerMainBilding _playerMainBilding;

    public override void InstallBindings()
    {
        PlayerMainBilding playerMainBilding = Container.InstantiatePrefabForComponent<PlayerMainBilding>(_playerMainBilding, _playerMainBilding.transform.position, Quaternion.identity, null);

        Container.Bind<PlayerMainBilding>().FromInstance(playerMainBilding).AsSingle().NonLazy();
    }
}