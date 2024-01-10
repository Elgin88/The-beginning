using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuildingSustem : MonoBehaviour
{
    private PlacementSystem _placementSystem;
    
    private void Construct(PlacementSystem placementSystem)
    {
        _placementSystem = placementSystem;
    }
}
