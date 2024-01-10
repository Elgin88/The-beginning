using UnityEngine;
using Zenject;

public class TestInstaller : MonoInstaller
{
    [SerializeField] private Cube _cube;
    [SerializeField] private Squere _squere;

    public override void InstallBindings()
    {
        Container.Bind<Cube>().FromInstance(_cube).AsSingle();
        Container.Bind<Squere>().FromInstance(_squere).AsSingle();
        //Container.QueueForInject(Cube);
    }
}
