using Assets.Scripts.PlayerComponents;
using Assets.Scripts.Props;
using Assets.Scripts.Props.Chest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal class ResoorceBuilding : Building
    {
        [SerializeField] private List<ChestSpawnPoint> _chestSpawnPoints;
        [SerializeField] private Chest _prefabOfChest;


        private int _currentIndexOfChestSpawnPoint;
        //private int _currentPlayersCoins;
        private int _firstChestSpawnPoint = 0;

        public static Action Created;

        private void Start()
        {
            Created?.Invoke();
        }

        public override bool IsPlayerObject { get => throw new System.NotImplementedException(); }

        private void OnEnable()
        {
            BuildingUI.SpawnChestButtonClicked += SpawnChest; 
            ChestSpawnPointsActivator.Activated += AddChestSpawnPoint;
        }

        private void OnDisable()
        {
            BuildingUI.SpawnChestButtonClicked -= SpawnChest;
            ChestSpawnPointsActivator.Activated -= AddChestSpawnPoint;
        }

        private void SpawnChest(PlayerWallet wallet, int costToBuy)   //проверка денег игрока
        {
            if(_chestSpawnPoints.Count != 0)
            {  
                if(wallet.Coins >= costToBuy)
                {
                    int _lastChestSpawnPoint = _chestSpawnPoints.Count;
                    _currentIndexOfChestSpawnPoint = Random.Range(_firstChestSpawnPoint, _lastChestSpawnPoint);

                    Chest chestToSpawn = Instantiate(_prefabOfChest, _chestSpawnPoints[_currentIndexOfChestSpawnPoint].transform);
                    chestToSpawn.SetCountOfCoins(_chestSpawnPoints[_currentIndexOfChestSpawnPoint].CoinsOfChest);
                    _chestSpawnPoints.RemoveAt(_currentIndexOfChestSpawnPoint);
                    wallet.SpendCoins(costToBuy);
                }   
            }  
        }

        private void AddChestSpawnPoint(List<ChestSpawnPoint> points)
        {
            _chestSpawnPoints = points;
        }
    }
}