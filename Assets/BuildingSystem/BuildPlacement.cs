using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPlacement : MonoBehaviour
{
    [SerializeField] private GameObject _spotToBuildIndicator, _cellIndicator;
    [SerializeField] private BuildPointInput _inputedPoint;
    [SerializeField] private Grid _grid;
    [SerializeField] private BuildingsContainer _buildingContainer;
    [SerializeField] private List<GameObject> _gridVisualisations;

    private Vector3 _inputPosition;
    private Vector3Int _gridCellPosition;
    private int _selectedBuildIndex = -1;
    private GameObject _selectedBuilding;
    private GridData _groundData, _buildingData;
    private Renderer _previewRenderer;
    private bool _placementValidity;
    private GridData _selectedGridData;
    private List<GameObject> _placedBuildings = new();


    private void Start()
    {
        StopPlacement();

        _groundData = new();
        _buildingData = new();
        _previewRenderer = _cellIndicator.GetComponentInChildren<Renderer>();
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

        _placementValidity = CheckPlacementValidity(_inputPosition, _gridCellPosition, _selectedBuildIndex);
        _previewRenderer.material.color = _placementValidity ? Color.white : Color.red;

        _spotToBuildIndicator.transform.position = _inputPosition;
        _cellIndicator.transform.position = _grid.CellToWorld(_gridCellPosition);
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
        
        _cellIndicator.SetActive(true);
    }

    private void TurnVisualisationOff()
    {
        for (int i = 0; i < _gridVisualisations.Count; i++)
        {
            _gridVisualisations[i].SetActive(false);
        }
       
        _cellIndicator.SetActive(false);
    }

    private void PlaceStruture()
    {
        int correctionNumber = 1;


        if (_inputedPoint.IsPointerOverUI())
        {
            return;
        }
            _inputPosition = _inputedPoint.DetermineSpotToBuild();
            _gridCellPosition = _grid.WorldToCell(_inputPosition);
        //звук постройки

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
    }

    private void StopPlacement()
    {
        _selectedBuildIndex = -1;

        TurnVisualisationOff();
        UnSubscribeOnInpudActions();
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
