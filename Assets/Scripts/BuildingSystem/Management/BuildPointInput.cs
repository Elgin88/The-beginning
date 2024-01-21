using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.BuildingSystem.Management
{
    internal class BuildPointInput : MonoBehaviour
    {
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private LayerMask _placeToBuild;

        private Vector3 _lastPosition;

        public event Action OnClicked, OnCancel;

        private void Update()
        {
            DeterminePlayersInput();
        }

        public bool IsPointerOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void DeterminePlayersInput()  //���� �� �� ������ ������� ����� � � �������������� ����� - ���������� � ���������� �����������
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnClicked?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Escape))   //���� �� �� ������ ������� ����� � � �������������� ����� - ���������� � ���������� �����������
            {
                OnCancel?.Invoke();
            }
        }

        public Vector3 DetermineSpot()
        {
            Vector3 mousePosition = Input.mousePosition;  //���� �� �� ������ ������� ����� � � �������������� ����� - ���������� � ���������� �����������
                                                          //mousePosition.z = _sceneCamera.nearClipPlane;

            Ray ray = _sceneCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, _placeToBuild))
            {
                _lastPosition = hit.point;
            }

            return _lastPosition;
        }
    }
}