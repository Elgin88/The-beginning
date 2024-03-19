using Assets.Scripts.BuildingSystem.Buildings;
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
        private const int _buildButtonIndex = 1;
        private const int _spawnUnitButtonIndex = 2;
        private const int _spawnChestButtonIndex = 3;
        
        [SerializeField] private Button _buildButton;
        [SerializeField] private Button _spawnUnitButton;
        [SerializeField] private Button _spawnChestButton;

        private string _buttonText = "Построить за ";
        private int _currentPlayersCoins;

        public Action BuildButtonClicked;
        public static Action SpawnUnitButtonClicked;
        public static Action SpawnChestButtonClicked;

        private void OnEnable()
        {
            _buildButton.onClick.AddListener(OnBuildButtonClicked);  //передвать деньги от игрока через _currentPlayersCoins
            _spawnUnitButton.onClick.AddListener(OnSpawnButtonClicked);  //передвать деньги от игрока через _currentPlayersCoins
            _spawnChestButton.onClick.AddListener(OnSpawnChestButtonClicked);  //передвать деньги от игрока через _currentPlayersCoins
            ButtonEventer.PlayerWentIn += ToggleButton;
            ButtonEventer.PlayerWentOut += ToggleButton;
           
        }

        private void OnDisable()
        {
            _buildButton.onClick.RemoveListener(OnBuildButtonClicked);
            _spawnUnitButton.onClick.RemoveListener(OnSpawnButtonClicked);
            _spawnChestButton.onClick.RemoveListener(OnSpawnChestButtonClicked);
            ButtonEventer.PlayerWentIn -= ToggleButton;
            ButtonEventer.PlayerWentOut -= ToggleButton;
        }

        public void SetButtonText(int costOfBuilding)
        {
            _buildButton.GetComponentInChildren<TMP_Text>().text = _buttonText + costOfBuilding;
        }

        private void OnBuildButtonClicked()
        {
            BuildButtonClicked?.Invoke();   //передвать деньги от игрока через _currentPlayersCoins
        }

        private void OnSpawnButtonClicked()
        {
            SpawnUnitButtonClicked?.Invoke();  //передвать деньги от игрока через _currentPlayersCoins
        }

        private void OnSpawnChestButtonClicked()
        {
            SpawnChestButtonClicked?.Invoke();  //передвать деньги от игрока через _currentPlayersCoins
        }

        public void ToggleButton(int indexOfButton, bool isTurnedOn)   //принимать деньги игрока и записывать в _currentPlayersCoins
        {
            switch (indexOfButton) 
            {
                case _buildButtonIndex:
                    _buildButton.gameObject.SetActive(isTurnedOn);
                    break;
                case _spawnUnitButtonIndex:
                    _spawnUnitButton.gameObject.SetActive(isTurnedOn);
                    break;
                case _spawnChestButtonIndex:
                    _spawnChestButton.gameObject.SetActive(isTurnedOn);
                    break;
            } 
        }
    }
}