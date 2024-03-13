using Assets.Scripts.PlayerUnits;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.BuildingSystem
{
    internal class BuildingUI : MonoBehaviour
    {
        [SerializeField] private Button _buildButton;
        [SerializeField] private Button _spawnUnitButton;


        private string _buttonText = "Построить за ";
        private int _buildButtonIndex = 1;
        

        public Action BuildButtonClicked;
        public static Action SpawnButtonClicked;

        private void OnEnable()
        {
            _buildButton.onClick.AddListener(OnBuildButtonClicked);
            _spawnUnitButton.onClick.AddListener(OnBuildButtonClicked);
            UnitsFactory.PlayerWentIn += ActiveButton;
            UnitsFactory.PlayerWentOut += DeActiveButton;
        }

        private void OnDisable()
        {
            _buildButton.onClick.RemoveListener(OnBuildButtonClicked);
            _spawnUnitButton.onClick.RemoveListener(OnSpawnButtonClicked);
            UnitsFactory.PlayerWentIn -= ActiveButton;
            UnitsFactory.PlayerWentOut -= DeActiveButton;
        }

        public void SetButtonText(int costOfBuilding)
        {
            _buildButton.GetComponentInChildren<TMP_Text>().text = _buttonText + costOfBuilding;
        }

        private void OnBuildButtonClicked()
        {
            BuildButtonClicked?.Invoke(); 
        }

        private void OnSpawnButtonClicked()
        {
            SpawnButtonClicked?.Invoke();
        }

        public void ActiveButton(int indexOfButton)
        {
            if (indexOfButton == _buildButtonIndex)
            {
                _buildButton.gameObject.SetActive(true);
            }
            else
            {
                _spawnUnitButton.gameObject.SetActive(true);
            }  
        }


        public void DeActiveButton(int indexOfButton)
        {
            if (indexOfButton == _buildButtonIndex)
            {
                _buildButton.gameObject.SetActive(false);
            }
            else
            {
                _spawnUnitButton.gameObject.SetActive(false);
            }
        }
    }
}