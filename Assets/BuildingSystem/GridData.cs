using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    private Dictionary<Vector3Int, PlacementInfo> _placedBuildings = new();
    private List<Vector3Int> _positionToOccupy = new();

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
        _positionToOccupy = CalculatePositions(gridPosition, buildingSize);
       
        PlacementInfo placementInfo = new PlacementInfo(_positionToOccupy, id, placedBuildingIndex);

        foreach (var position in _positionToOccupy)
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
        _positionToOccupy = CalculatePositions(gridPosition, buildingSize);

        foreach (var position in _positionToOccupy)
        {
            if (_placedBuildings.ContainsKey(position))
            {
                return false;
            }
        }
        return true;
    }
}

public class PlacementInfo
{
    public List<Vector3Int> OccuiedPositions;

    public int Id { get; private set; }
    public int PlacedBuildingIndex { get; private set; }

    public PlacementInfo(List<Vector3Int> occuiedPositions, int id, int placedBuildingIndex)
    {
        OccuiedPositions = occuiedPositions;
        Id = id;
        PlacedBuildingIndex = placedBuildingIndex;
    }
}


