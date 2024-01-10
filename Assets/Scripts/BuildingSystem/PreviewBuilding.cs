using UnityEngine;
using Zenject;

public class PreviewBuilding : MonoBehaviour
{
    private GameObject _cellIndicator;
    private GameObject _preview;
    private int _defautlValueOfSize = 1;
    private Vector2Int _defaultSize;
    private SpriteRenderer _cellIndicatorRenderer;
    private Color _currentCellIndicatorColor;
    private float priviewYOffset = 0.08f;

    [Inject]
    private void Construct(GameObject cellIndicator)
    {
        _cellIndicator = cellIndicator;
        Debug.Log("превью инжект");
    }

    private void Start()
    {
        _cellIndicator.SetActive(false);
        _cellIndicatorRenderer = _cellIndicator.GetComponentInChildren<SpriteRenderer>();
        Debug.Log("превью старт");
    }

    private void MoveCellInndicator(Vector3 position)
    {
        _cellIndicator.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        _preview.transform.position = new Vector3(position.x, position.y + priviewYOffset, position.z);
    }

    private void _ChangeCellIndicatorSize(Vector2Int size)
    {   
        if (size.x > 0 || size.y > 0)
        {
            _cellIndicator.transform.localScale = new Vector3(size.x, _defautlValueOfSize, size.y);
        }
    }

    public void SetCellIndicatorColor(bool validity)
    {
        _currentCellIndicatorColor = validity ? Color.white : Color.red;
        _currentCellIndicatorColor.a = 0.5f;


        _cellIndicatorRenderer.material.color = _currentCellIndicatorColor;
    }

    public void StartShowRemovePreview()
    {
        _defaultSize = new Vector2Int(_defautlValueOfSize, _defautlValueOfSize);
        _cellIndicator.SetActive(true);
        _ChangeCellIndicatorSize(_defaultSize);
    }

    public void StartShowBuildPreview(GameObject prefab, Vector2Int size)
    {
        _preview = Instantiate(prefab);
        _ChangeCellIndicatorSize(size);
        _cellIndicator.SetActive(true);
    }

    public void StopShowBuildPreview()
    {
        _cellIndicator.SetActive(false);

        if (_preview != null)
        {
            Destroy(_preview);
        } 
    }

    public void UpdatePositionOfPreview(Vector3 position)
    {
       if(_preview != null)
        {
            MovePreview(position);
        }
       
        MoveCellInndicator(position);
    }
}

