using UnityEngine;

namespace Assets.Scripts.ConStants
{
    internal static class AnimatorHash
    {
        public static int Hit = Animator.StringToHash("Hit");
        public static int Speed = Animator.StringToHash("Speed");
        public static int Death = Animator.StringToHash("Death");
        public static int Restart = Animator.StringToHash("Restart");
        public static int Intercat = Animator.StringToHash("Interact");
        public static int EquipBow = Animator.StringToHash("Equip Bow");
        public static int BowAttack = Animator.StringToHash("Bow Attack");
        public static int EquipSword = Animator.StringToHash("Equip Sword");
        public static int SwordAttack = Animator.StringToHash("Sword Attack");

        public static string Attack = " Attack";
    }
}