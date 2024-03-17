using System;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal class MainBuilding : Building
    {
        public override bool IsPlayerObject => gameObject;
    }
}