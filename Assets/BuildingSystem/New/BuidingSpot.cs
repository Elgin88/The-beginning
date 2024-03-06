using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BuildingSystem.New
{
    internal class BuidingSpot : MonoBehaviour
    {
        [SerializeField] private int _id;
        [SerializeField] private Transform _spotToPlace;

        public bool IsOccupied;

        public int Id { get { return _id; } }
        public Transform SpotToPlace { get {  return _spotToPlace; } }

        private void OnEnable()
        {
            NewBuiding.Destroyed += FreeSpotToBuild;
        }

        private void OnDisable()
        {
            NewBuiding.Destroyed -= FreeSpotToBuild;
        }
       
        private void FreeSpotToBuild(int buidingID)
        {
            if(IsOccupied == true && buidingID == _id)
            {
                IsOccupied = false;
            }
        }
    }
}