using Assets.Scripts.Bildings;
using UnityEngine;
using Zenject;

public class PlayerMainBildingInstaller : MonoInstaller
{
    [SerializeField] private PlayerMainBilding _playerMainBilding;

    public override void InstallBindings()
    {
        Container.Bind<PlayerMainBilding>().FromInstance(_playerMainBilding).AsSingle();
        Container.QueueForInject(_playerMainBilding);
    }
}