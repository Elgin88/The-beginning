using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    private List<GameObject> _placedBuildings = new();

    public int PlaceBuilding(GameObject prefab, Vector3 position)
    {
        int correctionNumber = 1;
        GameObject selectedBuilding = Instantiate(prefab);
        selectedBuilding.transform.position = position;
        _placedBuildings.Add(selectedBuilding);
        return _placedBuildings.Count - correctionNumber;
    }

    public void RemoveBuilding(int indexToRemove)
    {
        if(_placedBuildings.Count <= indexToRemove || _placedBuildings[indexToRemove] == null)
        {
            return;
        }
        else
        {
            Destroy(_placedBuildings[indexToRemove]);
            _placedBuildings[indexToRemove] = null;
        }
    }
}
