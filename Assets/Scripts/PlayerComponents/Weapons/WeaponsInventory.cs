using System.Collections.Generic;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class WeaponsInventory
    {
        private List<Weapon> _weapons = new List<Weapon>();

        public int GetWeaponsCount() => _weapons.Count;

        public WeaponsInventory(Weapon[] baseWeapons) 
        {
            foreach (var weapon in baseWeapons)
            {
                AddWeapon(weapon);
            }

            foreach (var weapon in _weapons)
            {
                weapon.gameObject.SetActive(false);
            }
        }

        public Weapon ChangeWeapon(int index)
        {
            _weapons[index].gameObject.SetActive(true);

            return _weapons[index];
        }

        public void AddWeapon(Weapon weapon)
        {
            _weapons.Add(weapon);
        }
    }
}
