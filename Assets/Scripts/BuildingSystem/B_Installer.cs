using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class B_Installer : MonoInstaller
{
    [SerializeField] private BuildingHandler _buildingHandler;
    [SerializeField] private PreviewBuilding _previewBuilding;
    [SerializeField] private BuildingPlacer _buildingPlacer;
    [SerializeField] private GameObject _cellIndicator;
    [SerializeField] private PlacementSystem _placementSystem;
    [SerializeField] private Grid _grid;
   // [SerializeField] private Transform _gridTransformPosition;

    [Header("Replaceable Components")]    

    [SerializeField] private Camera _sceneCamera;
    [SerializeField] private LayerMask _placeToBuild;
    //[SerializeField] private List<GameObject> _gridVisualisations;
    [SerializeField] private BuildingsContainer _buildingContainer;

    public override void InstallBindings()
    {
        BindBuildingHandler();
        BindViewSystem();
        BindBuildingSystem();
    }

    private void BindBuildingHandler()
    {
        Container.Bind<Camera>().FromInstance(_sceneCamera).AsSingle();
        Container.Bind<LayerMask>().FromInstance(_placeToBuild).AsSingle();
        Container.Bind<BuildingHandler>().FromInstance(_buildingHandler).AsSingle();
    }

    private void BindViewSystem()
    {
        Container.Bind<Grid>().FromInstance(_grid).AsSingle();
        Container.Bind<BuildingsContainer>().FromInstance(_buildingContainer).AsSingle();

        //List<GameObject> gridVisualisations = new();


        //for (int i = 0; i < _gridVisualisations.Count; i++)
        //{
        //    gridVisualisations[i] = Container.InstantiatePrefabForComponent<GameObject>(_gridVisualisations[i]);
        //}
        
        
        //List<GameObject> gridVisualisations = Container.InstantiatePrefabForComponent<GameObject>(_gridVisualisations);

       // Container.Bind<List<GameObject>>().FromInstance(_gridVisualisations).AsSingle();
       // Container.Bind<Transform>().FromInstance(_gridTransformPosition).AsSingle();


        Container.Bind<GameObject>().FromInstance(_cellIndicator).AsSingle();
        Container.Bind<PreviewBuilding>().FromInstance(_previewBuilding).AsSingle();
    }
    private void BindBuildingSystem()
    {
        Container.Bind<BuildingPlacer>().FromInstance(_buildingPlacer).AsSingle();
        Container.Bind<PlacementSystem>().FromInstance(_placementSystem).AsSingle().NonLazy();
    }
}
