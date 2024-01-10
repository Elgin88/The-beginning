using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlacementSystem : MonoBehaviour
{
    private BuildingHandler _buildingHandler;
    private Grid _grid;
    private BuildingsContainer _buildingContainer;
    [SerializeField] private List<GameObject> _gridVisualisations;
    private PreviewBuilding _previewBuilding;
    private BuildingPlacer _buildingPlacer;
    private Transform _gridTransform;

    private Vector3 _inputPosition;
    private Vector3Int _gridCellPosition;
    private GridData _groundData, _buildingData;

    private Vector3Int _lastSettedPosition = Vector3Int.zero;
    private IBuildingState _buildingState;

    [Inject]
    private void Construct(BuildingHandler buildingHandler, Grid grid, BuildingsContainer buildingContainer, List<GameObject> gridVisualisations,
                                                                                PreviewBuilding previewBuilding, BuildingPlacer buildingPlacer) //Transform gridTransform
    {
        _buildingHandler = buildingHandler;
        _grid = grid;
        _buildingContainer = buildingContainer;
        _gridVisualisations = gridVisualisations;
        _previewBuilding = previewBuilding;
        _buildingPlacer = buildingPlacer;
        //_gridTransform = gridTransform;
    }


    private void Start()
    {
        EndStateAction();

        Debug.Log("система стартанула");

        _groundData = new();
        _buildingData = new();
    }

    private void Update()
    {
        SelectSpotToBuild();
    }

    private void OnEnable()
    {
        PanelButton.PanelActivated += TurnVisualisationOn;
        PanelButton.PanelDeActivated += EndStateAction;
    }

    private void OnDisable()
    {
        PanelButton.PanelActivated -= TurnVisualisationOn;
        PanelButton.PanelDeActivated -= EndStateAction;
    }

    private void SelectSpotToBuild()
    {
        if (_buildingState == null)
        {
            return;
        }

        _inputPosition = _buildingHandler.DetermineSpot();
        _gridCellPosition = _grid.WorldToCell(_inputPosition);

        if(_lastSettedPosition != _gridCellPosition)
        {
            _buildingState.UpdateState(_gridCellPosition);
            _lastSettedPosition = _gridCellPosition;
        }    
    }

    private void TurnVisualisationOn()
    {
        for (int i = 0; i < _gridVisualisations.Count; i++)
        {
            
            //if( _gridVisualisations[i] != null)
            //{
            //    Instantiate(_gridVisualisations[i], _gridTransform);
            //}
            _gridVisualisations[i].SetActive(true);
            //Instantiate(_gridVisualisations[i],);
        }
    }

    private void TurnVisualisationOff()
    {
        for (int i = 0; i < _gridVisualisations.Count; i++)
        {
            _gridVisualisations[i].SetActive(false);
        }
    }

    private void StartStateAction()
    {
        if (_buildingHandler.IsPointerOverUI())
        {
            return;
        }
            _inputPosition = _buildingHandler.DetermineSpot();
            _gridCellPosition = _grid.WorldToCell(_inputPosition);

        _buildingState.OnAction(_gridCellPosition);
    }

    private void EndStateAction()
    {
        if (_buildingState == null)
        {
            return;
        }

        TurnVisualisationOff();
        _buildingState.EndState();
        UnSubscribeOnInpudActions();
        _lastSettedPosition = Vector3Int.zero;
        _buildingState = null;
    }

    private void SubscribeOnInpudActions()
    {
        _buildingHandler.OnPlaced += StartStateAction;
       // _buildingHandler.OnCancel += EndStateAction;
    }

    private void UnSubscribeOnInpudActions()
    {
        _buildingHandler.OnPlaced -= StartStateAction;
        //_buildingHandler.OnCancel -= EndStateAction;
    }

    public void StartPlacement(int id)
    {
        EndStateAction();
        TurnVisualisationOn();
        _buildingState = new PlaceBuildingState(id, _grid, _previewBuilding, _buildingContainer, _groundData, _buildingData, _buildingPlacer);
        SubscribeOnInpudActions();
    }

    public void RemoveBuilding()
    {
        EndStateAction();
        TurnVisualisationOn();
        _buildingState = new RemoveBuildingState(_grid,_previewBuilding,_groundData,_buildingData,_buildingPlacer);
        SubscribeOnInpudActions();
    }
}
