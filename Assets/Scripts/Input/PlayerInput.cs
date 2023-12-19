using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Input
{
    internal class PlayerInput : MonoBehaviour
    {
        private InputActions _inputActions;
        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;

        private Vector2 _moveDirection;

        private void Awake()
        {
            _inputActions = new InputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void Update()
        {
           _moveDirection = _inputActions.Player.Move.ReadValue<Vector2>();

            OnMoveInput(_moveDirection);
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        public void Init(PlayerMover playerMover)
        {
            _playerMover = playerMover;
        }

        private void OnMoveInput(Vector2 direction)
        {
            _playerMover.Move(direction);
        }
    }
}
