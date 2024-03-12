using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal class Barracks : Building
    {
        [SerializeField] private Transform _spotOfRespawnUnits;
       
        public override bool IsPlayerObject { get => throw new System.NotImplementedException(); }

        public static Action<Transform> Created;

        private void Start()
        {
            Created?.Invoke(_spotOfRespawnUnits);
        }
    }
}