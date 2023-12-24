using UnityEngine;

public class PreviewBuilding : MonoBehaviour
{
    [SerializeField] private float priviewYOffset = 0.08f;
    [SerializeField] private GameObject _cellIndicator;
    
    private GameObject _previewBuilding;
    private SpriteRenderer _previewRenderer;
    private int _defautlValueOfSize = 1;
    private Vector2Int _defaultSize;

    private void Start()
    {
        _cellIndicator.SetActive(false);
        _previewRenderer = _cellIndicator.GetComponentInChildren<SpriteRenderer>();
    }

    private void SetCellIndicatorColor(bool validity)
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

    private void _ChangeCellIndicatorSize(Vector2Int size)
    {   
        if (size.x > 0 || size.y > 0)
        {
            _cellIndicator.transform.localScale = new Vector3(size.x, _defautlValueOfSize, size.y);
            _previewRenderer.material.mainTextureScale = size;
        }
    }

    public void StartShowRemovePreview()
    {
        _defaultSize = new Vector2Int(_defautlValueOfSize, _defautlValueOfSize);
        _cellIndicator.SetActive(true);
        _ChangeCellIndicatorSize(_defaultSize);
        SetCellIndicatorColor(false);
    }

    public void StartShowBuildPreview(GameObject prefab, Vector2Int size)
    {
        _previewBuilding = Instantiate(prefab);
        _ChangeCellIndicatorSize(size);
        _cellIndicator.SetActive(true);
    }

    public void StopShowBuildPreview()
    {
        _cellIndicator.SetActive(false);

        if (_previewBuilding != null)
        {
            Destroy(_previewBuilding);
        } 
    }

    public void UpdatePositionOfPreview(Vector3 position, bool validity)
    {
       if(_previewBuilding != null)
        {
            MovePreview(position);
        }
       
        MoveCellInndicator(position);
        SetCellIndicatorColor(validity);
    }
}

