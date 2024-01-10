using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GridVisualizationInstaller : MonoInstaller
{
    [SerializeField] private List<GameObject> _gridVisualisations;

    public override void InstallBindings()
    {
        Container.Bind<List<GameObject>>().FromInstance(_gridVisualisations).AsSingle();
    }
}
