using UnityEngine;
using Zenject;
using Assets.Scripts.PlayerUnits;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerInput : MonoBehaviour
    {
        private InputActions _inputActions;

        private PlayerMovement _playerMover;
        private PlayerAttacker _playerAttacker;
        private SelectedUnitsHandler _selectedUnitsHandler;

        private Vector2 _moveDirection;

        private void OnEnable()
        {
            _inputActions = new InputActions();
            _inputActions.Enable();

            Cursor.visible = true;

            _inputActions.Player.Attack.performed += ctx => OnAttackInput();
            _inputActions.Player.ChangeWeapon.performed += ctx => OnChangeWeaponInput();
            _inputActions.Player.MoveUnits.performed += ctx => OnMoveUnits();
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
            if (_selectedUnitsHandler.IsAnyUnitSelected())
                _selectedUnitsHandler.MoveUnits();
        }

        [Inject]
        private void Construct(PlayerMovement movement, PlayerAttacker attacker, SelectedUnitsHandler selectedUnitsHandler)
        {
            _playerMover = movement;
            _playerAttacker = attacker;
            _selectedUnitsHandler = selectedUnitsHandler;
        }
    }
}