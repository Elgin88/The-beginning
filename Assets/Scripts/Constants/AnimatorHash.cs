using UnityEngine;

namespace Assets.Scripts.Constants
{
    internal static class AnimatorHash
    {
        public static int Hit = Animator.StringToHash("Hit");
        public static int Speed = Animator.StringToHash("Speed");
        public static int Death = Animator.StringToHash("Death");
        public static int Attack = Animator.StringToHash("Attack");
        public static int Moving = Animator.StringToHash("IsMoving");
        public static int Restart = Animator.StringToHash("Restart");
        public static int Intercat = Animator.StringToHash("Interact");
        public static int EquipBow = Animator.StringToHash("Equip Bow");
        public static int BowAttack = Animator.StringToHash("Bow Attack");
        public static int EquipSword = Animator.StringToHash("Equip Sword");
        public static int AttackSpeed = Animator.StringToHash("AttackSpeed");
        public static int SwordAttack = Animator.StringToHash("Sword Attack");

        public static string SwordAttackString = "Sword Attack";
        public static string BowAttackString = "Bow Attack";
    }
}