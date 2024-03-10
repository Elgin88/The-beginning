using UnityEngine;
using Assets.Scripts.PlayerComponents;
using System.Collections.Generic;
using System;


namespace Assets.Scripts.BuildingSystem
{
    internal class BuildPoint : MonoBehaviour
    {
        [SerializeField] private Transform _spotToPlaceBuilding;
        [SerializeField] private int _buildingPointIndex;
        [SerializeField] private GameObject _iconOfBuildPoint;
        [SerializeField] private int _costToBuild;

        private bool _isOccupied;
        private int speedOfRotateVisualObject = 200;
        

        public Transform SpotToPlaceBuilding => _spotToPlaceBuilding;
        public int BuildingPointIndex => _buildingPointIndex;
        public bool IsOccupied => _isOccupied;
        public int CostToBuild => _costToBuild;


        public Action<Transform> PlayerWentIn;
        public Action PlayerWentOut; //Transform


        private void Update()
        {
            RotateIconOfBuildingPoint();
        }

        private void RotateIconOfBuildingPoint()
        {
            _iconOfBuildPoint.transform.Rotate(0, speedOfRotateVisualObject * Time.deltaTime, 0);
        }

        private void OnEnable()
        {
            Building.Destroyed += FreeSpotToBuild;
        }

        private void OnDisable()
        {
            Building.Destroyed -= FreeSpotToBuild;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.gameObject.TryGetComponent(out Player player))
            {
                if (_isOccupied == false) // тут проверять хватает ли денег у игрока сравнивая с _buildingUI.BuidingCost
                {
                    PlayerWentIn?.Invoke(transform);
                }
            }
        }

        private void FreeSpotToBuild(Transform buidingTransform)
        {
            if (_isOccupied == true && buidingTransform.position == _spotToPlaceBuilding.position)
            {
                _isOccupied = false;
                ActiveIconOfBuildPoint();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.gameObject.TryGetComponent(out Player player))
            {
                PlayerWentOut?.Invoke(); //transform
            }
        }

        public void TakeSpot()
        {
            _isOccupied = true;
        }

        public void ActiveIconOfBuildPoint()
        {
            _iconOfBuildPoint.SetActive(true);
        }

        public void TryToDeActiveIconOfBuildPoint()
        {
            if (_iconOfBuildPoint.gameObject.activeSelf == true)
            {
                _iconOfBuildPoint.SetActive(false);
            }
        }
    }
}