using Assets.Scripts.BuildingSystem.Buildings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Props.Chest
{
    internal class ChestSpawnPointsActivator : MonoBehaviour
    {
        [SerializeField] private List<ChestSpawnPoint> _chestSpawnPoints;

        public static Action<List<ChestSpawnPoint>> Activated;

        private void OnEnable()
        {
            ResoorceBuilding.Created += ActivePoints;
        }

        private void OnDisable()
        {
            ResoorceBuilding.Created -= ActivePoints;
        }

        private void ActivePoints()
        {
            Activated?.Invoke(_chestSpawnPoints);
        }
    }
}