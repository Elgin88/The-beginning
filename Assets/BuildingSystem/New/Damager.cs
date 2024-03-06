using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BuildingSystem.New
{
    internal class Damager : MonoBehaviour
    {
        public void GiveDamage(NewBuiding building)
        {
            building.TakeDamage(5);
        }
    }
}