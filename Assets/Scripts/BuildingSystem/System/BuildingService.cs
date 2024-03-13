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
        // сделать переменную для денег игрока
        private int _buildButtonIndex = 1;

        private void OnEnable()
        {
            SignToBuildingsPointEvents();
            _buildingUI.BuildButtonClicked += Build;
        }

        private void OnDisable()
        {
            UnSignToBuildingsPointEvents();
            _buildingUI.BuildButtonClicked -= Build;
        }
       
        private void Build()   //тут сравнивнить деньги с _buildingUI.BuidingCost
        {

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
                            _buildingUI.DeActiveButton(_buildButtonIndex);
                        }
                    }
                }
            }
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

        private void OnPlayerWentIn(Transform spotOfPlayer)  // тут получить деньги от игрока
        {
            _currentPlayersTransform = spotOfPlayer;

            for (int i = 0; i < _buildPoints.Count; i++)
            {
                if (_buildPoints[i].transform == spotOfPlayer)
                {
                    _buildingUI.ActiveButton(_buildButtonIndex);
                    _buildingUI.SetButtonText(_buildPoints[i].CostToBuild);     
                }
            }
        }

        private void OnPlayerWentOut() 
        {
            _buildingUI.DeActiveButton(_buildButtonIndex);
        }
    }
}

   

   