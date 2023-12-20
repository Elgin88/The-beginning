using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BuildingSystem/ButtonsIndexesContainer")]
public class BuildingButtonsIndexesContainer : ScriptableObject
{
    [field: SerializeField] public int MainBuildingButtonId { get; private set; }
    [field: SerializeField] public int TowerButtonId { get; private set; }
    [field: SerializeField] public int ResourceBuildingButtonId { get; private set; }
    [field: SerializeField] public int BarracksButtonId { get; private set; }
}
