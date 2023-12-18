using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPointDetecter : MonoBehaviour
{
    [SerializeField] private Camera _sceneCamera;
    [SerializeField] private LayerMask _placeToBuild;

    private Vector3 _lastPosition;

   
    //���� �� �� ������ ������� ����� � � �������������� �����
    
    public Vector3 GetSelectedSpotToBuild()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = _sceneCamera.nearClipPlane;

        Ray ray = _sceneCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 100, _placeToBuild))
        {
            _lastPosition = hit.point;
        }

        return _lastPosition;
    }
}
