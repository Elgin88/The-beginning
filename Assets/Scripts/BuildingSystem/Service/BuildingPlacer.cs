using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Service
{
    internal class BuildingPlacer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _placedBuildings = new();

        private int _yOffset = 1;
        
        public int PlaceBuilding(GameObject prefab, Vector3 position)
        {
            int correctionNumber = 1;
            GameObject selectedBuilding = Instantiate(prefab);
           // selectedBuilding.transform.position = position;
            selectedBuilding.transform.position = new Vector3(position.x, position.y - _yOffset, position.z);
            _placedBuildings.Add(selectedBuilding);
            return _placedBuildings.Count - correctionNumber;
        }

        public void RemoveBuilding(int indexToRemove)
        {
            if (_placedBuildings.Count <= indexToRemove || _placedBuildings[indexToRemove] == null)
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
}