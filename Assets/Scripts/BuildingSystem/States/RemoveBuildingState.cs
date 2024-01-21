using Assets.Scripts.BuildingSystem.Service;
using Assets.Scripts.BuildingSystem.View;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.States
{
    internal class RemoveBuildingState : IBuildingState
    {
        private static int _defaultValueBuildIndexToRemove = -1;

        private int _buildIndexToRemove = _defaultValueBuildIndexToRemove;
        private Grid _grid;
        private PreviewBuilding _previewBuilding;
        private GridData _groundData, _buildingData;
        private BuildingPlacer _buildingPlacer;
        private GridData _selectedGridData;
        private Vector2Int _defaultSize;
        private int _defautlValueOfSize = 1;
        private Vector3 _currentGridCellPosition;

        public RemoveBuildingState(Grid grid, PreviewBuilding previewBuilding, GridData groundData, GridData buildingData, BuildingPlacer buildingPlacer)
        {
            _grid = grid;
            _previewBuilding = previewBuilding;
            _groundData = groundData;
            _buildingData = buildingData;
            _buildingPlacer = buildingPlacer;

            _previewBuilding.StartShowRemovePreview();
        }

        public void EndState()
        {
            _previewBuilding.StopShowBuildPreview();
        }

        public void OnAction(Vector3Int gridPosition)
        {
            _defaultSize = new Vector2Int(_defautlValueOfSize, _defautlValueOfSize);

            _selectedGridData = null;

            if (_buildingData.CanPlaceBuilding(gridPosition, _defaultSize) == false)
            {
                _selectedGridData = _buildingData;
            }
            else if (_groundData.CanPlaceBuilding(gridPosition, _defaultSize) == false)
            {
                _selectedGridData = _groundData;
            }

            if (_selectedGridData == null)
            {
                throw new Exception("Unable to remove.");
            }
            else
            {
                _buildIndexToRemove = _selectedGridData.GetIndexOfBuildingToRemove(gridPosition);

                if (_buildIndexToRemove == _defaultValueBuildIndexToRemove)
                {
                    return;
                }
                else
                {
                    _selectedGridData.RemoveBuilding(gridPosition);
                    _buildingPlacer.RemoveBuilding(_buildIndexToRemove);
                }
            }

            _currentGridCellPosition = _grid.CellToWorld(gridPosition);
            _previewBuilding.UpdatePositionOfPreview(_currentGridCellPosition);
        }

        public void UpdateState(Vector3Int gridPosition)
        {
            _previewBuilding.UpdatePositionOfPreview(_grid.CellToWorld(gridPosition));
        }
    }
}