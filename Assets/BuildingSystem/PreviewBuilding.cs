using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewBuilding : MonoBehaviour
{
    [SerializeField] private float priviewYOffset = 0.08f;
    [SerializeField] private GameObject _cellIndicator;
    
    private GameObject _previewBuilding;
    private SpriteRenderer _previewRenderer;

    private void Start()
    {
        _cellIndicator.SetActive(false);
        _previewRenderer = _cellIndicator.GetComponentInChildren<SpriteRenderer>();
    }

    private void SetCellColor(bool validity)
    {
        _previewRenderer.material.color = validity ? Color.white : Color.red;
    }

    private void MoveCellInndicator(Vector3 position)
    {
        _cellIndicator.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        _previewBuilding.transform.position = new Vector3(position.x, position.y + priviewYOffset, position.z);
    }

    private void _ChangeCellIndicaterSize(Vector2Int size)
    {
        if (size.x > 0 || size.y > 0)
        {
            _cellIndicator.transform.localScale = new Vector3(size.x, 1, size.y);
            _previewRenderer.material.mainTextureScale = size;
        }
    }

    public void StartShowBuildPreview(GameObject prefab, Vector2Int size)
    {
        _previewBuilding = Instantiate(prefab);
        _ChangeCellIndicaterSize(size);
        _cellIndicator.SetActive(true);
    }

    public void StopShowBuildPreview()
    {
        _cellIndicator.SetActive(false);
        Destroy(_previewBuilding);
    }

    public void UpdatePositionOfPreview(Vector3 position, bool validity)
    {
        MovePreview(position);
        MoveCellInndicator(position);
        SetCellColor(validity);
    }
}

