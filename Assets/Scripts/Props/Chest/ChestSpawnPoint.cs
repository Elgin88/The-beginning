using Assets.Scripts.BuildingSystem.Buildings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Props.Chest
{
    internal class ChestSpawnPoint : MonoBehaviour
    {
        [SerializeField] private int _coinsOfChest;

        public int CoinsOfChest { get { return _coinsOfChest; } }
    }
}