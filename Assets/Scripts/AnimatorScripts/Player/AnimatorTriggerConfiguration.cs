using Assets.Scripts.ConStants;
using Assets.Scripts.PlayerComponents.Weapons;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.AnimatorScripts.Player
{
    internal class AnimatorTriggerConfiguration
    {
        private Dictionary<Type, int> _attackTriggers = new Dictionary<Type, int>();
        private Dictionary<Type, int> _changeWeaponTriggers = new Dictionary<Type, int>();

        public AnimatorTriggerConfiguration()
        {
            _attackTriggers[typeof(Bow)] = AnimatorHash.BowAttack;
            _attackTriggers[typeof(Sword)] = AnimatorHash.SwordAttack;

            _changeWeaponTriggers[typeof(Bow)] = AnimatorHash.EquipBow;
            _changeWeaponTriggers[typeof(Sword)] = AnimatorHash.EquipSword;
        }

        public int GetAttackTrigger(Type weaponType)
        {
            return GetTrigger(_attackTriggers, weaponType);
        }

        public int GetChangeWeaponTrigger(Type weaponType)
        {
            return GetTrigger(_changeWeaponTriggers, weaponType);
        }

        private int GetTrigger(Dictionary<Type, int> triggerMap, Type weaponType)
        {
            if (triggerMap.TryGetValue(weaponType, out int trigger))
            {
                return trigger;
            }
            else
            {
                throw new Exception("No such weapon map");
            }
        }
    }
}