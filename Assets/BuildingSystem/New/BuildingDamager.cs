using Assets.BuildingSystem.New;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class BuildingDamager : MonoBehaviour
{
    private int _damage = 5;
   
    public void GiveDamage(NewBuiding objectToDestroy)
    {
        objectToDestroy.TakeDamage(_damage);

    }
}
