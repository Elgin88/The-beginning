using UnityEngine;
using Assets.Scripts.Input;

namespace Assets.Scripts.Player
{
    internal class PlayerBootstrap : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private PlayerInput _input;

        private void Awake()
        {
            _input.Init(new PlayerMover(_player, 8, 2));
        }
    }
}
