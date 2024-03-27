using Agava.WebUtility;
using Assets.Scripts.GameLogic.Utilities;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerUnits;
using Tayx.Graphy.Graph;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.PlayerInput
{
    internal class MobileInput : MonoBehaviour
    {
        [SerializeField] private Joystick _joystick;
        [SerializeField] private Button _attack;
        [SerializeField] private Button _changeWeapon;
        [SerializeField] private SelectedUnitsHandler _selectedUnitsHandler;
        [SerializeField] private LayerMask _ground;

        private PlayerMovement _playerMover;
        private PlayerAttacker _playerAttacker;
        private Vector2 _moveDirection;

        private WorldPointFinder _worldPointFinder;

        private float _doubleTapThreshold = 0.3f;
        private float _lastTapTime;

        private void OnEnable()
        {
            //gameObject.SetActive(Device.IsMobile);

            _worldPointFinder = new WorldPointFinder(_ground);

            _attack.onClick.AddListener(OnAttackInput);
            _changeWeapon.onClick.AddListener(OnChangeWeaponInput);
        }

        private void FixedUpdate()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    if (Time.time - _lastTapTime <= _doubleTapThreshold)
                    {
                        _lastTapTime = 0;
                        OnMoveUnits(_worldPointFinder.GetPosition(touch.position));
                    }
                    else
                    {
                        _lastTapTime = Time.time;
                    }
                }
            }

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

        private void OnMoveUnits(Vector3 position)
        {
            _selectedUnitsHandler.MoveUnits(position);
        }

        [Inject]
        private void Construct(PlayerMovement movement, PlayerAttacker attacker)
        {
            _playerMover = movement;
            _playerAttacker = attacker;
        }
    }
}
