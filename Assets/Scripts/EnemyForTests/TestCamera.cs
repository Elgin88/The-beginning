using UnityEngine;

namespace Assets.Scripts.Tests
{
    internal class TestCamera: MonoBehaviour
    {
        [SerializeField] private GameObject _orc;

        private void Update()
        {
            transform.position = new Vector3(_orc.transform.position.x, 10, _orc.transform.position.z - 12);
        }
    }
}