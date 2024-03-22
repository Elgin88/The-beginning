using Assets.Scripts.EnemyNamespace;
using UnityEngine;
using Zenject;

public class EnemyFactoryInstaller : MonoInstaller
{
    [SerializeField] private EnemyFactory _enemyFactory;

    public override void InstallBindings()
    {
        BindEnemyFactory();
    }

    private void BindEnemyFactory()
    {
        Container.Bind<EnemyFactory>().FromInstance(_enemyFactory).AsSingle();
        Container.QueueForInject(_enemyFactory);
    }
}