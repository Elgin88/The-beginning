using UnityEngine;

namespace Assets.Scripts.Tests
{
    internal class TestCamera: MonoBehaviour
    {
        [SerializeField] private GameObject _gameObject;
        [SerializeField] private float deltaX;
        [SerializeField] private float deltaY;
        [SerializeField] private float deltaZ;

        private void Update()
        {
            transform.position = new Vector3(_gameObject.transform.position.x + deltaX, _gameObject.transform.position.y + deltaY, _gameObject.transform.position.z + deltaZ);
        }
    }
}