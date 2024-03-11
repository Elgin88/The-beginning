using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem
{
    internal class BuildingService : MonoBehaviour
    {
        [SerializeField] private BuildingUI _buildingUI;
        [SerializeField] private List<BuildPoint> _buildPoints;  
        [SerializeField] private List<Building> _buildings;

        private Transform _currentPlayersTransform;

        private void OnEnable()
        {
            SignToBuildingsPointEvents();
            _buildingUI.ButtonClicked += Build;
        }

        private void OnDisable()
        {
            UnSignToBuildingsPointEvents();
            _buildingUI.ButtonClicked -= Build;
        }
       
        private void Build()
        {
            Debug.Log("Кнопка работает из сервиса, но не строит");

            for (int i = 0; i < _buildPoints.Count; i++)
            {
                if (_buildPoints[i].SpotToPlaceBuilding != null && _buildPoints[i].IsOccupied == false && _currentPlayersTransform == _buildPoints[i].transform)
                {
                    for (int j = 0; j < _buildings.Count; j++)
                    {
                        if (_buildPoints[i].BuildingPointIndex == _buildings[j].IndexOfBuilding)
                        {
                            Instantiate(_buildings[j], _buildPoints[i].SpotToPlaceBuilding);
                            _buildPoints[i].TakeSpot();
                            _buildPoints[i].TryToDeActiveIconOfBuildPoint();
                            _buildingUI.TryToDeActiveButton();
                        }
                    }
                }
            }

            Debug.Log("Кнопка строит");
        }

        private void SignToBuildingsPointEvents()
        {
            for (int i = 0; i < _buildPoints.Count; i++)
            {
                _buildPoints[i].PlayerWentIn += OnPlayerWentIn;
                _buildPoints[i].PlayerWentOut += OnPlayerWentOut;
            }
        }

        private void UnSignToBuildingsPointEvents()
        {
            for (int i = 0; i < _buildPoints.Count; i++)
            {
                _buildPoints[i].PlayerWentIn -= OnPlayerWentIn;
                _buildPoints[i].PlayerWentOut -= OnPlayerWentOut;
            }
        }

        private void OnPlayerWentIn(Transform spotOfPlayer)
        {
            _currentPlayersTransform = spotOfPlayer;

            for (int i = 0; i < _buildPoints.Count; i++)
            {
                if (_buildPoints[i].transform == spotOfPlayer)
                {
                    _buildingUI.ActiveButton();
                    _buildingUI.SetButtonText(_buildPoints[i].CostToBuild);     
                }
            }
        }

        private void OnPlayerWentOut() 
        {
            _buildingUI.TryToDeActiveButton();
        }
    }
}

   

   