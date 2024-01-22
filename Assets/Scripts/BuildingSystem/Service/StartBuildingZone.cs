using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.PlayerComponents;
using UnityEngine.UI;

public class StartBuildingZone : MonoBehaviour
{
    public static Action PlayerWentIn;    // ��� ����� ������ ������, ������� ����� ����� ������ � ������� � �������, ����� ���������� ������� "���������"
    public static Action PlayerWentOut;
    

    private void OnTriggerEnter(Collider other)
    {
        if(other != null && other.gameObject.TryGetComponent(out Player player))
        {
             PlayerWentIn?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.gameObject.TryGetComponent(out Player player))
        {
            PlayerWentOut?.Invoke();
        }
    }
}
