using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerUnits;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal class Barracks : Building
    {
        [SerializeField] private UnitsFactory _unitsFactory;

        public override bool IsPlayerObject { get => throw new System.NotImplementedException(); }


        private void OnEnable()
        {
            BuildingUI.SpawnUnitButtonClicked += SpawnUnit;
        }

        private void OnDisable()
        {
            BuildingUI.SpawnUnitButtonClicked -= SpawnUnit;
        }

        private void SpawnUnit(PlayerWallet wallet, int costToBuy)  
        {
            if(wallet.Coins >= costToBuy)
            {
                _unitsFactory.Spawn();
                wallet.SpendCoins(costToBuy);
            }  
        }
    }
}