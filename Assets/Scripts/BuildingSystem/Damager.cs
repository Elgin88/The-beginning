using Assets.BuildingSystem.New;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem
{
    internal class Damager : MonoBehaviour
    {
        public void GiveDamage(Building building)
        {
            building.TakeDamage(5);
        }
    }
}