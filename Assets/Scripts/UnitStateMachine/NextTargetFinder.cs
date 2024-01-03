using UnityEngine;

namespace Scripts.UnitStateMachine
{
    public class NextTargetFinder : MonoBehaviour
    {
        [SerializeField] private GameObject _playerMainBilding;

        private GameObject _nextTarget;

        public GameObject NextTarget => _nextTarget;

        private void Start()
        {
            _nextTarget = _playerMainBilding;
        }
    }
}