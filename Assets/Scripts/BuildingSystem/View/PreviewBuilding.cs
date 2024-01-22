using UnityEngine;

namespace Assets.Scripts.BuildingSystem.View
{
    internal class PreviewBuilding : MonoBehaviour
    {
        [SerializeField] private float priviewYOffset = 0.08f;
        [SerializeField] private GameObject _cellIndicator;

        private GameObject _previewBuilding;
        private int _defautlValueOfSize = 1;
        private Vector2Int _defaultSize;
        private SpriteRenderer _cellIndicatorRenderer;
        private Color _currentCellIndicatorColor;

        private void Start()
        {
            _cellIndicator.SetActive(false);
            _cellIndicatorRenderer = _cellIndicator.GetComponentInChildren<SpriteRenderer>();
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

        public void UpdatePositionOfPreview(Vector3 position)
        {
            if (_previewBuilding != null)
            {
                MovePreview(position);
            }

            MoveCellInndicator(position);
        }
    }
}