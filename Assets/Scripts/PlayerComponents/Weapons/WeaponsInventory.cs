using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons
{
    internal class WeaponsInventory : MonoBehaviour
    {
        [SerializeField] private List<Weapon> _weapons;

        private Weapon _currentWeapon;

        public int GetWeaponsCount() => _weapons.Count;

        public Weapon ChangeWeapon()
        {
            DisableWeapons();

            for (int i = 0;  i < _weapons.Count; i++)
            {
                if (_currentWeapon == _weapons[i])
                {
                    int weaponIndex = i;

                    _currentWeapon = weaponIndex < _weapons.Count - 1 ? _weapons[++weaponIndex] : _weapons[0];

                    _currentWeapon.gameObject.SetActive(true);

                    return _currentWeapon;
                }
            }

            throw new System.Exception();
        }

        public void AddWeapon(Weapon weapon)
        {
            _weapons.Add(weapon);
        }

        public Weapon Init()
        {
            _currentWeapon = _weapons[0];

            return _currentWeapon;
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
