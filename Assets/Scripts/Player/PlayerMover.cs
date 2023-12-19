using UnityEngine;

namespace Assets.Scripts.Player
{
    internal class PlayerMover 
    {
        private Player _player;
        private float _moveSpeed;
        private float _rotationSpeed;

        public PlayerMover(Player player, float moveSpeed, float rotationSpeed)
        {
            _player = player;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
        }

        public void Move(Vector2 direction)
        {
            float scaledMoveSpeed = _moveSpeed * Time.deltaTime;
            Vector3 movementVector = new Vector3(direction.x, 0, direction.y);

            _player.transform.position += movementVector * scaledMoveSpeed;

            Rotate(movementVector);
        }

        private void Rotate(Vector3 movementVector)
        {
            if (movementVector.sqrMagnitude == 0)
                return;

            Quaternion rotation = Quaternion.LookRotation(movementVector);
            _player.transform.rotation = Quaternion.RotateTowards(_player.transform.rotation, rotation, _rotationSpeed);
        }
    }
}
