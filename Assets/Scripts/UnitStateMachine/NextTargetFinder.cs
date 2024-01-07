using Assets.Scripts.Bildings;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UnitStateMachine
{
    internal class NextTargetFinder : MonoBehaviour
    {
        [Inject] private PlayerMainBilding _playerMainBilding;

        public PlayerMainBilding PlayerMainBilding => _playerMainBilding;
    }
}