using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Constants;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerUnits;
using Assets.Scripts.UI;
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
        [SerializeField] private Button _spawnChestButton;

        private PlayerWallet _currentPlayersWallet;
        private int _currentCostToBuy;

        public Action<PlayerWallet> BuildButtonClicked;
        public static Action<PlayerWallet, int> SpawnUnitButtonClicked;
        public static Action<PlayerWallet, int> SpawnChestButtonClicked;

        private void OnEnable()
        {
            _buildButton.onClick.AddListener(OnBuildButtonClicked);  //передвать деньги от игрока через _currentPlayersCoins
            _spawnUnitButton.onClick.AddListener(OnSpawnButtonClicked);  //передвать деньги от игрока через _currentPlayersCoins
            _spawnChestButton.onClick.AddListener(OnSpawnChestButtonClicked);  //передвать деньги от игрока через _currentPlayersCoins
            ButtonBuyEventer.PlayerWentIn += ToggleButton;
            ButtonBuyEventer.PlayerWentOut += ToggleButton;
           
        }

        private void OnDisable()
        {
            _buildButton.onClick.RemoveListener(OnBuildButtonClicked);
            _spawnUnitButton.onClick.RemoveListener(OnSpawnButtonClicked);
            _spawnChestButton.onClick.RemoveListener(OnSpawnChestButtonClicked);
            ButtonBuyEventer.PlayerWentIn -= ToggleButton;
            ButtonBuyEventer.PlayerWentOut -= ToggleButton;
        }

        private void SetButtonText(Button activeButton,string title, int costToBuy)
        {
            activeButton.GetComponentInChildren<TMP_Text>().text = title + costToBuy;
        }

        private void OnBuildButtonClicked()
        {
            BuildButtonClicked?.Invoke(_currentPlayersWallet);   //передвать деньги от игрока через _currentPlayersCoins
        }

        private void OnSpawnButtonClicked()
        {
            SpawnUnitButtonClicked?.Invoke(_currentPlayersWallet, _currentCostToBuy);  //передвать деньги от игрока через _currentPlayersCoins 
        }

        private void OnSpawnChestButtonClicked()
        {
            SpawnChestButtonClicked?.Invoke(_currentPlayersWallet, _currentCostToBuy);  //передвать деньги от игрока через _currentPlayersCoins
        }

        public void ToggleButton(int indexOfButton, PlayerWallet wallet, int costToBuy, bool isTurnedOn)   //принимать деньги игрока и записывать в _currentPlayersCoins
        {
            _currentPlayersWallet = wallet;
            _currentCostToBuy = costToBuy;

            switch (indexOfButton) 
            {
                case BuildingUiHash.BuildButtonIndex:
                    _buildButton.gameObject.SetActive(isTurnedOn);
                    SetButtonText(_buildButton, BuildingUiHash.BuildButtonText, costToBuy);
                    break;
                case BuildingUiHash.SpawnUnitButtonIndex:
                    _spawnUnitButton.gameObject.SetActive(isTurnedOn);
                    SetButtonText(_spawnUnitButton, BuildingUiHash.SpawnUnitButtonText, costToBuy);
                    break;
                case BuildingUiHash.SpawnChestButtonIndex:
                    _spawnChestButton.gameObject.SetActive(isTurnedOn);
                    SetButtonText(_spawnChestButton, BuildingUiHash.SpawnChestButtonText, costToBuy);
                    break;
            } 
        }
    }
}