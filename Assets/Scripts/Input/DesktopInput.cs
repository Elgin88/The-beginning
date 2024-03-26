using UnityEngine;
using Zenject;
using Assets.Scripts.PlayerUnits;
using Assets.Scripts.PlayerComponents;

namespace Assets.Scripts.Input
{
    internal class DesktopInput : MonoBehaviour
    {
        [SerializeField] private SelectedUnitsHandler _selectedUnitsHandler;
        [SerializeField] private Joystick _joystick;

        private InputActions _inputActions;
        private PlayerMovement _playerMover;
        private PlayerAttacker _playerAttacker;

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
            _selectedUnitsHandler.MoveUnits();
        }

        [Inject]
        private void Construct(PlayerMovement movement, PlayerAttacker attacker)
        {
            _playerMover = movement;
            _playerAttacker = attacker;
        }
    }
}