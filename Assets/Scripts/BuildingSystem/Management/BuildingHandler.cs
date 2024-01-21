using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.BuildingSystem.Management
{
    [RequireComponent(typeof(InputActions))]
    internal class BuildingHandler : MonoBehaviour
    {
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private LayerMask _placeToBuild;

        private Vector2 _inputPosition;
        private Vector3 _lastPosition;
        private Vector3 _positionToPlace;
        private InputActions _inputActions;
        private float OffsetZ = 0.03f;
        private int _rayLength = 100;

        public event Action OnPlaced;

        private void Awake()
        {
            _inputActions = new InputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            _inputActions.BuildingSystem.PlaceBuilding.performed += OnPlace;
            _inputPosition = _inputActions.BuildingSystem.DeterminePosition.ReadValue<Vector2>();

        }

        private void OnPlace(InputAction.CallbackContext context)
        {
            OnPlaced?.Invoke();
        }

        public Vector3 DetermineSpot()
        {
            _positionToPlace = new Vector3(_inputPosition.x, _inputPosition.y, OffsetZ);

            Ray ray = _sceneCamera.ScreenPointToRay(_positionToPlace);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _rayLength, _placeToBuild))
            {
                _lastPosition = hit.point;
            }

            return _lastPosition;
        }

        public bool IsPointerOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}