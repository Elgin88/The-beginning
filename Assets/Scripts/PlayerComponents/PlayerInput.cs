using UnityEngine;
using Assets.Scripts.Movement;
using Zenject;
using Assets.Scripts.PlayerUnits.Utilities;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerInput : MonoBehaviour
    {
        private InputActions _inputActions;

        private PlayerMovement _playerMover;
        private PlayerAttacker _playerAttacker;

        private Vector2 _moveDirection;

        private void Start()
        {
            _inputActions = new InputActions();

            _inputActions.Enable();
        }

        private void FixedUpdate()
        {
            _moveDirection = _inputActions.Player.Move.ReadValue<Vector2>();
            _inputActions.Player.Attack.performed += ctx => OnAttackInput();
            _inputActions.Player.ChangeWeapon.performed += ctx => OnChangeWeaponInput();

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

        private void OnAttackInput()
        {
            _playerAttacker.Attack();
        }
        
        private void OnChangeWeaponInput()
        {
            _playerAttacker.ChangeWeapon();
        }

        private void OnMoveUnits()
        {

        }

        [Inject]
        private void Construct(PlayerMovement movement, PlayerAttacker attacker)
        {
            _playerMover = movement;
            _playerAttacker = attacker;
        }
    }
}