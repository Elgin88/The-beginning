using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPlacement : MonoBehaviour
{
    [SerializeField] private GameObject _spotToBuildIndicator, _cellIndicator;
    [SerializeField] private BuildPointDetecter _pointDetecter;
    [SerializeField] private Grid _grid;

    private Vector3 _inputPosition;
    private Vector3Int _gridCellPosition;

    private void Update()
    {
        _inputPosition = _pointDetecter.GetSelectedSpotToBuild();

        _gridCellPosition = _grid.WorldToCell(_inputPosition);

       _spotToBuildIndicator.transform.position = _inputPosition;
        _cellIndicator.transform.position = _grid.CellToWorld(_gridCellPosition); 
    }
}
