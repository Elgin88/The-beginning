using Assets.Scripts.PlayerComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class ButtonBuyEventer : MonoBehaviour
    {
        [SerializeField] private int _buttonIndex;
        [SerializeField] private int _costToBuy;

        private bool _isPlayerIn;
        private int _currentPlayerCoins;


        public static Action<int, PlayerWallet, int, bool> PlayerWentIn;   //����� ���������� ������ ������ ����� ��� ���� int
        public static Action<int, PlayerWallet, int, bool> PlayerWentOut;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Player player)) // ������ ��� �������� ����� ���
            {
                _isPlayerIn = true;
                PlayerWentIn?.Invoke(_buttonIndex, player.Wallet, _costToBuy, _isPlayerIn);
                Debug.Log("������ ����� ��� ������� - " + player.Wallet.Coins);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Player player))
            {
                _isPlayerIn = false;
                PlayerWentIn?.Invoke(_buttonIndex, player.Wallet, _costToBuy, _isPlayerIn);
            }
        }
    }
}