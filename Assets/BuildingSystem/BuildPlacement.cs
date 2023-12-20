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


    private void Start()
    {
        StopPlacement();
    }

    private void Update()
    {

        if (_selectedBuildIndex < 0)
        {
            return;
        }

        Vector3 inputPosition = _inputedPoint.DetermineSpotToBuild();
        Vector3Int gridCellPosition = _grid.WorldToCell(inputPosition);
        _spotToBuildIndicator.transform.position = inputPosition;
        _cellIndicator.transform.position = _grid.CellToWorld(gridCellPosition);

        // SelectSpotToBuild();
    }

    private void SelectSpotToBuild()
    {
        if (_selectedBuildIndex < 0)
        {
            return;
        }

        _inputPosition = _inputedPoint.DetermineSpotToBuild();

        _gridCellPosition = _grid.WorldToCell(_inputPosition);

        _spotToBuildIndicator.transform.position = _inputPosition;
        _cellIndicator.transform.position = _grid.CellToWorld(_gridCellPosition);
    }

    private void TurnVisualisationOn()
    {
        
        for(int i = 0; i < _gridVisualisations.Count; i++)
        {
            _gridVisualisations[i].SetActive(true);
        }
        
       // _gridVisualisations.SetActive(true);
        
        
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
        //Debug.Log("������ ��������� - " + _selectedBuildIndex);
        Debug.Log("���� ��������� - ");

        //if (_inputedPoint.IsPointerOverUI())
        //{
        //    Debug.Log("�� ���� ���������, � �� �����");
        //    return;
        //}
        //else
        //{

            Vector3 inputPosition = _inputedPoint.DetermineSpotToBuild();
            Vector3Int gridCellPosition = _grid.WorldToCell(inputPosition);
            Debug.Log("������ ��������� - " + _selectedBuildIndex);
            GameObject SELECTEDBUILDING = Instantiate(_buildingContainer.BuildingInfo[_selectedBuildIndex].Prefab);
            Debug.Log(SELECTEDBUILDING.gameObject.name);
            SELECTEDBUILDING.transform.position = _grid.CellToWorld(gridCellPosition);




            //_inputPosition = _inputedPoint.DetermineSpotToBuild();
            //_gridCellPosition = _grid.WorldToCell(_inputPosition);



            //_selectedBuilding = Instantiate(_buildingContainer.BuildingInfo[_selectedBuildIndex].Prefab);

           

            //_spotToBuildIndicator.transform.position = _inputPosition;
           // _selectedBuilding.transform.position = _grid.CellToWorld(_gridCellPosition);
       // }
    }

    private void StopPlacement()
    {
        _selectedBuildIndex = -1;

        TurnVisualisationOff();
        _inputedPoint.OnClicked -= PlaceStruture;
        _inputedPoint.OnCancel -= StopPlacement;
        
        //TurnVisualisationOff();
        //UnSubscribeOnInpudActions();
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

       // Debug.Log("������ ��������� - " + _buildingContainer.BuildingInfo.FindIndex(build => build.Id);
        _selectedBuildIndex = _buildingContainer.BuildingInfo.FindIndex(build => build.Id == id);
        //Debug.Log("������ ��������� - " + _selectedBuildIndex);

        if (_selectedBuildIndex < 0)
        {
            Debug.Log("������ ��������� ������ ���� ");
            return;
        }
        else
        {
            Debug.Log("���� �������");
            TurnVisualisationOn();
            _inputedPoint.OnClicked += PlaceStruture;
            _inputedPoint.OnCancel += StopPlacement;

            //TurnVisualisationOn();
            //SubscribeOnInpudActions();
        }
    }
}
