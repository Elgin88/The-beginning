using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private BuildPointInput _inputedPoint;
    [SerializeField] private Grid _grid;
    [SerializeField] private BuildingsContainer _buildingContainer;
    [SerializeField] private List<GameObject> _gridVisualisations;
    [SerializeField] private PreviewBuilding _previewBuilding;
    [SerializeField] private BuildingPlacer _buildingPlacer;

    private Vector3 _inputPosition;
    private Vector3Int _gridCellPosition;
    private GridData _groundData, _buildingData;

    private Vector3Int _lastSettedPosition = Vector3Int.zero;
    private IBuildingState _buildingState;
    
    private void Start()
    {
        StopPlacement();

        _groundData = new();
        _buildingData = new();
    }

    private void Update()
    {
        SelectSpotToBuild();
    }

    private void SelectSpotToBuild()
    {
        if (_buildingState == null)
        {
            return;
        }

        _inputPosition = _inputedPoint.DetermineSpot();
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
            _gridVisualisations[i].SetActive(true);
        }
    }

    private void TurnVisualisationOff()
    {
        for (int i = 0; i < _gridVisualisations.Count; i++)
        {
            _gridVisualisations[i].SetActive(false);
        }
    }

    private void PlaceStruture()
    {
        if (_inputedPoint.IsPointerOverUI())
        {
            return;
        }
            _inputPosition = _inputedPoint.DetermineSpot();
            _gridCellPosition = _grid.WorldToCell(_inputPosition);

        _buildingState.OnAction(_gridCellPosition);
    }

    private void StopPlacement()
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
        _inputedPoint.OnClicked += PlaceStruture;
        _inputedPoint.OnCancel += StopPlacement;
    }

    private void UnSubscribeOnInpudActions()
    {
        _inputedPoint.OnClicked -= PlaceStruture;
        _inputedPoint.OnCancel -= StopPlacement;
    }

    public void StartPlacement(int id)
    {
        StopPlacement();
        TurnVisualisationOn();
        _buildingState = new PlaceBuildingState(id, _grid, _previewBuilding, _buildingContainer, _groundData, _buildingData, _buildingPlacer);
        SubscribeOnInpudActions();
    }

    public void RemoveBuilding()
    {
        StopPlacement();
        TurnVisualisationOn();
        _buildingState = new RemoveBuildingState(_grid,_previewBuilding,_groundData,_buildingData,_buildingPlacer);
        SubscribeOnInpudActions();
    }
}
