using Assets.Scripts.PlayerComponents.Weapons;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerAttacker
    {
        private WeaponsInventory _inventory;

        private Weapon _currentWeapon;

        public PlayerAttacker(WeaponsInventory inventory) 
        { 
            _inventory = inventory;

            _currentWeapon = inventory.ChangeWeapon(0);
        }

        public void Attack()
        {
            _currentWeapon.Attack();
        }
    }
}
