using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.PlayerComponents;
using UnityEngine.UI;

public class StartBuildingZone : MonoBehaviour
{
    public  Action PlayerWentIn;    // тут нужен префаб иконки, которая будет видна игроку и подходя к которой, будет появляться кнопака "Построить"
    public  Action PlayerWentOut;
    

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
