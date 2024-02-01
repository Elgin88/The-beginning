using UnityEngine;

namespace Assets.Scripts.PlayerUnits.Utilities
{
    internal class GlobalSelection : MonoBehaviour
    {
        [SerializeField] private LayerMask _mask;

        private SelectionDictionary _selectedTable;
        private RaycastHit _hit;

        private bool _dragSelect;

        private Vector2 _onMouseDownPosition;
        private Vector3 _mousePositionY;

        private void Start()
        {
            Cursor.visible = true;

            _selectedTable = new SelectionDictionary();
            _dragSelect = false;
        }

        public void OnRightMouseButtonDown(Vector2 mousePosition)
        {
            _onMouseDownPosition = mousePosition;

            if (mousePosition.magnitude > 40)
            {
                _dragSelect = true;
            }
        }

        public void OnRightMouseButtonUp()
        {
            if (_dragSelect == false)
            {
                Ray ray = Camera.main.ScreenPointToRay(_onMouseDownPosition);

                if (Physics.Raycast(ray, out _hit, 1000, _mask))
                {
                    if (_hit.transform.gameObject.TryGetComponent<Unit>(out Unit unit))
                    {
                        _selectedTable.AddSelected(unit);
                    }
                }
            }
        }
    }
}
