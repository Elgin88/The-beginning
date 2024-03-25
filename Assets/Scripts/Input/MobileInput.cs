using Agava.WebUtility;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerUnits;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.Input
{
    internal class MobileInput : MonoBehaviour
    {
        [SerializeField] private Joystick _joystick;
        [SerializeField] private Button _attack;
        [SerializeField] private Button _changeWeapon;
        [SerializeField] private SelectedUnitsHandler _selectedUnitsHandler;

        private PlayerMovement _playerMover;
        private PlayerAttacker _playerAttacker;
        private Vector2 _moveDirection;

        private void OnEnable()
        {
            //gameObject.SetActive(Device.IsMobile);

            _attack.onClick.AddListener(OnAttackInput);
            _changeWeapon.onClick.AddListener(OnChangeWeaponInput);
        }

        private void FixedUpdate()
        {
            Vector3 direction = Vector3.forward * _joystick.Vertical + Vector3.right * _joystick.Horizontal;

            _moveDirection = new Vector2(direction.x, direction.z);

            OnMoveInput(_moveDirection);
        }

        private void OnDisable()
        {
            _attack.onClick.RemoveListener(OnAttackInput);
            _changeWeapon.onClick.RemoveListener(OnChangeWeaponInput);
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
