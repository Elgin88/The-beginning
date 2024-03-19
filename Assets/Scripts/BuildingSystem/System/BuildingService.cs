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
        // ������� ���������� ��� ����� ������
        private int _buildButtonIndex = 1;
        private bool _canBuild;

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
       
        private void Build()   //��� ����������� ������ � _buildingUI.BuidingCost
        {
            _canBuild = false;
            
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
                            _buildingUI.ToggleButton(_buildButtonIndex, _canBuild);
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

        private void OnPlayerWentIn(Transform spotOfPlayer)  // ��� �������� ������ �� ������
        {
            _currentPlayersTransform = spotOfPlayer;
            _canBuild = true;

            for (int i = 0; i < _buildPoints.Count; i++)
            {
                if (_buildPoints[i].transform == spotOfPlayer)
                {
                    _buildingUI.ToggleButton(_buildButtonIndex, _canBuild);
                    _buildingUI.SetButtonText(_buildPoints[i].CostToBuild);     
                }
            }
        }

        private void OnPlayerWentOut() 
        {
            _canBuild = false;
            _buildingUI.ToggleButton(_buildButtonIndex, _canBuild);
        }
    }
}

   

   