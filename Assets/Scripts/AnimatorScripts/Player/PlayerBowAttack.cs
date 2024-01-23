using UnityEngine;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerComponents.Weapons;
using Zenject;

namespace Assets.Scripts.AnimatorScripts.Player
{
    internal class PlayerBowAttack : StateMachineBehaviour
    {
        private PlayerMovement _movement;
        private Mark _mark;
        private Vector3 _rotationOffset = new Vector3(0, 75, 0);

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _movement.StopMove();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _movement.RotateTowards(_mark.Target, _rotationOffset);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _movement.StartMove();
        }

        [Inject]
        private void Construct(PlayerMovement movement, Mark mark)
        {
            _movement = movement;
            _mark = mark;
        }
    }
}
