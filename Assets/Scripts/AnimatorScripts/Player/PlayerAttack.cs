using Assets.Scripts.PlayerComponents;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.AnimatorScripts.Player
{
    internal class PlayerAttack : StateMachineBehaviour
    {
        private PlayerMovement _movement;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _movement.StopMove();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _movement.StartMove();
        }

        [Inject]
        private void Construct(PlayerMovement movement)
        {
            _movement = movement;
        }
    }
}