using System;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal class MainBuilding : Building
    {
        [SerializeField] private Transform  _spotOfRespawnUnits;

        public override bool IsPlayerObject => gameObject;

        public static Action<Transform> Created;


        private void Start()
        {
            Created?.Invoke(_spotOfRespawnUnits);
        }
    }
}