using UnityEngine;
using Assets.Scripts.PlayerComponents;

namespace Assets.Scripts.Input
{
    [RequireComponent(typeof(Player))]
    internal class PlayerInput : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        private InputActions _inputActions;

        private Player _player;
        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;

        private Vector2 _moveDirection;

        private void Start()
        {
            _player = GetComponent<Player>();

            _inputActions = new InputActions();
            _playerAttacker = new PlayerAttacker();
            _playerMover = new PlayerMover(_player, _layerMask);

            _inputActions.Enable();
        }

        private void FixedUpdate()
        {
            _moveDirection = _inputActions.Player.Move.ReadValue<Vector2>();

            OnMoveInput(_moveDirection);
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void OnMoveInput(Vector2 direction)
        {
            _playerMover.Move(direction);
        }
    }
}