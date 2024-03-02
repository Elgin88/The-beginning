using Assets.Scripts.BuildingSystem.Management;
using Assets.Scripts.BuildingSystem.States;
using Assets.Scripts.BuildingSystem.View;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Service
{
    internal class PlacementSystem : MonoBehaviour
    {
        [SerializeField] private BuildingHandler _buildingHandler;
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
            EndStateAction();

            _groundData = new();
            _buildingData = new();
        }

        private void Update()
        {
            SelectSpotToBuild();
        }

        private void OnEnable()
        {
            BuildingUI.BuildPanelActivated += TurnVisualisationOn;
            BuildingUI.BuildPanelDeActivated += EndStateAction;
        }

        private void OnDisable()
        {
            BuildingUI.BuildPanelActivated -= TurnVisualisationOn;
            BuildingUI.BuildPanelDeActivated -= EndStateAction;
        }

        private void SelectSpotToBuild()
        {
            if (_buildingState == null)
            {
                return;
            }

            _inputPosition = _buildingHandler.DetermineSpot();
            _gridCellPosition = _grid.WorldToCell(_inputPosition);

            if (_lastSettedPosition != _gridCellPosition)
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
            _buildingState = new RemoveBuildingState(_grid, _previewBuilding, _groundData, _buildingData, _buildingPlacer);
            SubscribeOnInpudActions();
        }
    }
}