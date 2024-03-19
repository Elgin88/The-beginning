using UnityEngine;

namespace Assets.Scripts.Tests
{
    internal class TestCamera: MonoBehaviour
    {
        [SerializeField] private GameObject _gameObject;

        private float deltaX = 0;
        private float deltaY = 10;
        private float deltaZ = -10;

        private void Update()
        {
            if (_gameObject != null)
            {
                transform.position = new Vector3(_gameObject.transform.position.x + deltaX, _gameObject.transform.position.y + deltaY, _gameObject.transform.position.z + deltaZ);
            }
        }
    }
}