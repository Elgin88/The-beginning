using UnityEngine;

namespace Assets.Scripts.ConStants
{
    internal static class AnimatorHash
    {
        public static int SwordAttack = Animator.StringToHash("Sword Attack");
        public static int BowAttack = Animator.StringToHash("Bow Attack");
        public static int Intercat = Animator.StringToHash("Interact");
        public static int Restart = Animator.StringToHash("Restart");
        public static int Death = Animator.StringToHash("Death");
        public static int Speed = Animator.StringToHash("Speed");
        public static int Hit = Animator.StringToHash("Hit");

        public static string Attack = " Attack";
    }
}