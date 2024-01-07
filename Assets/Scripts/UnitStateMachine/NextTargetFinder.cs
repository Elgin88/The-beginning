using Assets.Scripts.Bildings;
using UnityEngine;
using Zenject;

namespace Scripts.UnitStateMachine
{
    public class NextTargetFinder : MonoBehaviour
    {
        [Inject] private PlayerMainBilding _playerMainBilding;

        public Vector3 PlayerMainBildingPosition => _playerMainBilding.transform.position;
    }
}