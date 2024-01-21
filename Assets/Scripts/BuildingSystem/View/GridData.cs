using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.View
{
    internal class GridData
    {
        private Dictionary<Vector3Int, PlacementInfo> _placedBuildings = new();
        private List<Vector3Int> _positionToPlace = new();
        private int _defaultValueOfBuildIndex = -1;

        private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int buildingSize)
        {
            List<Vector3Int> returnValueOfPosition = new();

            for (int x = 0; x < buildingSize.x; x++)
            {
                for (int y = 0; y < buildingSize.y; y++)
                {
                    returnValueOfPosition.Add(gridPosition + new Vector3Int(x, 0, y));
                }
            }
            return returnValueOfPosition;
        }

        public void AddBuilding(Vector3Int gridPosition, Vector2Int buildingSize, int id, int placedBuildingIndex)
        {
            _positionToPlace = CalculatePositions(gridPosition, buildingSize);

            PlacementInfo placementInfo = new PlacementInfo(_positionToPlace, id, placedBuildingIndex);

            foreach (var position in _positionToPlace)
            {
                if (_placedBuildings.ContainsKey(position))
                {
                    throw new Exception($"Dictionary already contains this cell position {position}");
                }

                _placedBuildings[position] = placementInfo;
            }
        }

        public bool CanPlaceBuilding(Vector3Int gridPosition, Vector2Int buildingSize)
        {
            _positionToPlace = CalculatePositions(gridPosition, buildingSize);

            foreach (var position in _positionToPlace)
            {
                if (_placedBuildings.ContainsKey(position))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetIndexOfBuildingToRemove(Vector3Int gridPosition)
        {
            if (_placedBuildings.ContainsKey(gridPosition) == false)
            {
                return _defaultValueOfBuildIndex;
            }
            else
            {
                return _placedBuildings[gridPosition].PlacedBuildingIndex;
            }
        }

        public void RemoveBuilding(Vector3Int gridPosition)
        {
            foreach (var position in _placedBuildings[gridPosition].PlacedPositions)
            {
                _placedBuildings.Remove(position);
            }
        }
    }

    public class PlacementInfo
    {
        public List<Vector3Int> PlacedPositions;

        public int Id { get; private set; }
        public int PlacedBuildingIndex { get; private set; }

        public PlacementInfo(List<Vector3Int> placedPositions, int id, int placedBuildingIndex)
        {
            PlacedPositions = placedPositions;
            Id = id;
            PlacedBuildingIndex = placedBuildingIndex;
        }
    }
}