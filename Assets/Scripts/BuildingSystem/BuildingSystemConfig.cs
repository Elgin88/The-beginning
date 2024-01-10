using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BuildingSystem/BuildingSystemConfig")]
public class BuildingSystemConfig : ScriptableObject
{
   // public BuildingSystemInfo BuildingInformation;

    [field: SerializeField] public Camera CurrentCamera { get; private set; }
    [field: SerializeField] public LayerMask BuildingLayerMask { get; private set; }
    [field: SerializeField] public BuildingHandler BuildingHandler { get; private set; }
    [field: SerializeField] public GameObject CellIndicator { get; private set; }
    [field: SerializeField] public BuildingsContainer BuildingContainer { get; private set; }
    [field: SerializeField] public GridData GridData { get; private set; }
    [field: SerializeField] public List<GameObject> GridVisualisations { get; private set; }
    [field: SerializeField] public PreviewBuilding PreviewBuilding { get; private set; }
    [field: SerializeField] public BuildingPlacer BuildingPlacer { get; private set; }
    [field: SerializeField] public PlacementSystem PlacementSystem { get; private set; }
}

//public class BuildingSystemInfo
//{
    
//}

