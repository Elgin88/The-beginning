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
        private int _currentPlayersCoins;
       
        public override bool IsPlayerObject { get => throw new System.NotImplementedException(); }


        private void OnEnable()
        {
            BuildingUI.SpawnUnitButtonClicked += SpawnUnit;
        }

        private void OnDisable()
        {
            BuildingUI.SpawnUnitButtonClicked -= SpawnUnit;
        }

        private void SpawnUnit()  // сделать проверку на деньги
        {
            _unitsFactory.Spawn();
        }
    }
}