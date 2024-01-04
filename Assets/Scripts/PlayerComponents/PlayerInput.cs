using UnityEngine;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.Movement;
using Zenject;

namespace Assets.Scripts.Input
{
    [RequireComponent(typeof(Player))]
    internal class PlayerInput : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        private InputActions _inputActions;

        private Player _player;
        private Moveable _playerMover;
        private PlayerAttacker _playerAttacker;

        private Vector2 _moveDirection;

        private void Start()
        {
            _player = GetComponent<Player>();

            _inputActions = new InputActions();
            _playerMover = new Moveable(_player, _layerMask);

            _inputActions.Enable();
        }

        private void FixedUpdate()
        {
            _moveDirection = _inputActions.Player.Move.ReadValue<Vector2>();
            _inputActions.Player.Attack.performed += ctx => OnAttackInput();

            OnMoveInput(_moveDirection);
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        [Inject]
        private void Construct(PlayerAttacker attacker)
        {
            _playerAttacker = attacker;

            attacker.ChangeWeapon();
        }

        private void OnMoveInput(Vector2 direction)
        {
            _playerMover.Move(direction);
        }

        private void OnAttackInput()
        {
            _playerAttacker.Attack();
        }
    }
}