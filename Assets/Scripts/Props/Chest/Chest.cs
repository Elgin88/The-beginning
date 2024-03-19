using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Props.Chest
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    internal class Chest : MonoBehaviour
    {
        private const string _isOpen = "IsOpen";

        [SerializeField] private int _coins;
        
        private Animator animator;
        private AudioSource _audiosourse;
        private bool _isEmpty = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            _audiosourse = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Player player) && _isEmpty == false)
            {
                GiveCoins(player);
            }
        }


        private void GiveCoins(Player player)
        {
            // вызвать метод взятия монет у игрока
            _isEmpty = true;
        }

        public void SetCountOfCoins(int coins)
        {
            _coins = coins;
        }
    }
}