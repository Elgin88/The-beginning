using Assets.Scripts.PlayerComponents.Weapons;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerAttacker
    {
        private WeaponsInventory _inventory;

        private Weapon _currentWeapon;

        public PlayerAttacker(WeaponsInventory inventory) 
        {
            _inventory = inventory;
        }

        public void Attack()
        {
            _currentWeapon.Attack();
        }

        public void ChangeWeapon()
        {
            _currentWeapon = _inventory.ChangeWeapon(1);
        }
    }
}
