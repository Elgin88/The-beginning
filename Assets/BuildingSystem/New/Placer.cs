using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BuildingSystem.New
{
    internal class Placer : MonoBehaviour
    {
        [SerializeField] private List<NewBuiding> _buidings;
        [SerializeField] private List<BuidingSpot> _buildingSpots;

        public void TryToPlaceBuilding(int buttonId)
        {
            for (int i = 0; i < _buidings.Count; i++)
            {
                for (int j = 0; j < _buildingSpots.Count; j++)
                {
                    if (_buidings[i].Id == _buildingSpots[j].Id && _buildingSpots[j].IsOccupied == false && buttonId == _buidings[i].Id)
                    {
                        PlaceBuilding(buttonId, _buidings[i], _buildingSpots[j].SpotToPlace);
                            _buildingSpots[j].IsOccupied = true;
                    }
                }
            }
        }

        private void PlaceBuilding(int buttonId,NewBuiding buiding, Transform spotToPlace)
        {   
            Instantiate(buiding, spotToPlace);
        }
    }
}