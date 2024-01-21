using Assets.Scripts.BuildingSystem.Service;
using Assets.Scripts.BuildingSystem.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.States
{
    internal class PlaceBuildingState : IBuildingState
    {
        private static int _defaultValueOfSelectedBuildIndex = -1;

        private int _selectedBuildIndex = _defaultValueOfSelectedBuildIndex;
        private int _id;
        private Grid _grid;
        private PreviewBuilding _previewBuilding;
        private BuildingsContainer _buildingContainer;
        private GridData _groundData, _buildingData;
        private BuildingPlacer _buildingPlacer;
        private GridData _selectedGridData;
        private bool _placementValidity;
        private bool _unableToPlace = false;
        private int _indexOfBuildToPlace;

        public PlaceBuildingState(int id, Grid grid, PreviewBuilding previewBuilding, BuildingsContainer buildingContainer, GridData groundData, GridData buildingData, BuildingPlacer buildingPlacer)
        {
            _id = id;
            _grid = grid;
            _previewBuilding = previewBuilding;
            _buildingContainer = buildingContainer;
            _groundData = groundData;
            _buildingData = buildingData;
            _buildingPlacer = buildingPlacer;

            _selectedBuildIndex = _buildingContainer.BuildingInformation.FindIndex(build => build.Id == id);

            if (_selectedBuildIndex > _defaultValueOfSelectedBuildIndex)
            {
                _previewBuilding.StartShowBuildPreview(_buildingContainer.BuildingInformation[_selectedBuildIndex].Prefab,
                            _buildingContainer.BuildingInformation[_selectedBuildIndex].Size);
            }
            else
            {
                throw new System.Exception($"No building with id {_id}");
            }
        }

        private bool IsPlaceReady(Vector3Int gridCellPosition, int selectedBuildIndex)
        {
            _selectedGridData = _buildingContainer.BuildingInformation[_selectedBuildIndex].Id == 0 ? _groundData : _buildingData;

            return _selectedGridData.CanPlaceBuilding(gridCellPosition, _buildingContainer.BuildingInformation[selectedBuildIndex].Size);
        }

        public void EndState()
        {
            _previewBuilding.StopShowBuildPreview();
        }

        public void OnAction(Vector3Int gridPosition)
        {
            _placementValidity = IsPlaceReady(gridPosition, _selectedBuildIndex);

            if (_placementValidity == false)
            {
                return;
            }

            _indexOfBuildToPlace = _buildingPlacer.PlaceBuilding(_buildingContainer.BuildingInformation[_selectedBuildIndex].Prefab, _grid.CellToWorld(gridPosition));
            _selectedGridData = _buildingContainer.BuildingInformation[_selectedBuildIndex].Id == 0 ? _groundData : _buildingData;

            _selectedGridData.AddBuilding(gridPosition,
                _buildingContainer.BuildingInformation[_selectedBuildIndex].Size,
                _buildingContainer.BuildingInformation[_selectedBuildIndex].Id,
                _indexOfBuildToPlace);

            _previewBuilding.UpdatePositionOfPreview(_grid.CellToWorld(gridPosition));
            _previewBuilding.SetCellIndicatorColor(_unableToPlace);
        }

        public void UpdateState(Vector3Int gridPosition)
        {
            _placementValidity = IsPlaceReady(gridPosition, _selectedBuildIndex);
            _previewBuilding.UpdatePositionOfPreview(_grid.CellToWorld(gridPosition));
            _previewBuilding.SetCellIndicatorColor(_placementValidity);
        }
    }
}