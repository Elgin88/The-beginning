using Assets.Scripts.PlayerComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEventer : MonoBehaviour
{
    [SerializeField] private int _buttonIndex;

    private bool _isPlayerIn;
    
    public static Action<int, bool> PlayerWentIn;   //также передавать деньги игрока через ещё один int
    public static Action<int, bool> PlayerWentOut;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player player)) // деньги для проверки взять тут
        {
            _isPlayerIn = true;
            PlayerWentIn?.Invoke(_buttonIndex, _isPlayerIn);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
            _isPlayerIn = false;
            PlayerWentOut?.Invoke(_buttonIndex, _isPlayerIn);
        }
    }
}
