using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPlacement : MonoBehaviour
{
    [SerializeField] private GameObject _spotToBuildIndicator; 
    [SerializeField] private BuildPointInput _inputedPoint;
    [SerializeField] private Grid _grid;
    [SerializeField] private BuildingsContainer _buildingContainer;
    [SerializeField] private List<GameObject> _gridVisualisations;
    [SerializeField] private PreviewBuilding _previewBuilding;

    private Vector3 _inputPosition;
    private Vector3Int _gridCellPosition;
    private int _selectedBuildIndex = -1;
    private GameObject _selectedBuilding; 
    private GridData _groundData, _buildingData;
    private bool _placementValidity;
    private GridData _selectedGridData;
    private List<GameObject> _placedBuildings = new();
    private Vector3Int _lastSettedPosition = Vector3Int.zero;


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
        if (_selectedBuildIndex < 0)
        {
            return;
        }

        _inputPosition = _inputedPoint.DetermineSpotToBuild();
        _gridCellPosition = _grid.WorldToCell(_inputPosition);

        if(_lastSettedPosition != _gridCellPosition)
        {
            _placementValidity = CheckPlacementValidity(_inputPosition, _gridCellPosition, _selectedBuildIndex);

            _spotToBuildIndicator.transform.position = _inputPosition;
            _previewBuilding.UpdatePositionOfPreview(_grid.CellToWorld(_gridCellPosition), _placementValidity);
            _lastSettedPosition = _gridCellPosition;
        }    
    }

    private bool CheckPlacementValidity(Vector3 inputPosition, Vector3Int gridCellPosition, int selectedBuildIndex)
    {
        _selectedGridData = _buildingContainer.BuildingInformation[_selectedBuildIndex].Id == 0 ? _groundData : _buildingData;

        return _selectedGridData.CanPlaceBuilding(gridCellPosition, _buildingContainer.BuildingInformation[_selectedBuildIndex].Size);
    }


    private void TurnVisualisationOn()
    {      
        for(int i = 0; i < _gridVisualisations.Count; i++)
        {
            _gridVisualisations[i].SetActive(true);
        }
        
        _previewBuilding.StartShowBuildPreview(_buildingContainer.BuildingInformation[_selectedBuildIndex].Prefab,
            _buildingContainer.BuildingInformation[_selectedBuildIndex].Size);
    }

    private void TurnVisualisationOff()
    {
        for (int i = 0; i < _gridVisualisations.Count; i++)
        {
            _gridVisualisations[i].SetActive(false);
        }
       
        _previewBuilding.StopShowBuildPreview();
    }

    private void PlaceStruture()
    {
        int correctionNumber = 1;
        bool unableToPlace = false;

        if (_inputedPoint.IsPointerOverUI())
        {
            return;
        }
            _inputPosition = _inputedPoint.DetermineSpotToBuild();
            _gridCellPosition = _grid.WorldToCell(_inputPosition);

        _placementValidity = CheckPlacementValidity(_inputPosition, _gridCellPosition, _selectedBuildIndex);

        if (_placementValidity == false)
        {
            return;
        }

        _selectedBuilding = Instantiate(_buildingContainer.BuildingInformation[_selectedBuildIndex].Prefab);
        _selectedBuilding.transform.position = _grid.CellToWorld(_gridCellPosition);
        _placedBuildings.Add(_selectedBuilding);
        _selectedGridData = _buildingContainer.BuildingInformation[_selectedBuildIndex].Id == 0 ? _groundData : _buildingData;
        
        _selectedGridData.AddBuilding(_gridCellPosition, 
            _buildingContainer.BuildingInformation[_selectedBuildIndex].Size,
            _buildingContainer.BuildingInformation[_selectedBuildIndex].Id, 
            _placedBuildings.Count - correctionNumber);
       
        _previewBuilding.UpdatePositionOfPreview(_grid.CellToWorld(_gridCellPosition), unableToPlace);
    }

    private void StopPlacement()
    {
        _selectedBuildIndex = -1;

        TurnVisualisationOff();
        UnSubscribeOnInpudActions();
        _lastSettedPosition = Vector3Int.zero;
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

        _selectedBuildIndex = _buildingContainer.BuildingInformation.FindIndex(build => build.Id == id);

        if (_selectedBuildIndex < 0)
        {
            return;
        }
        else
        {
            TurnVisualisationOn();
            SubscribeOnInpudActions();
        }
    }
}
