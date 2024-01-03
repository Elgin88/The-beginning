using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class WeaponsInventory : MonoBehaviour
    {
        [SerializeField] private List<Weapon> _weapons;

        public int GetWeaponsCount() => _weapons.Count;

        public Weapon ChangeWeapon(int index)
        {
            DisableWeapons();

            _weapons[index].gameObject.SetActive(true);

            return _weapons[index];
        }

        public void AddWeapon(Weapon weapon)
        {
            _weapons.Add(weapon);
        }

        private void DisableWeapons()
        {
            foreach (var weapon in _weapons)
            {
                weapon.gameObject.SetActive(false);
            }
        }
    }
}
