using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class UnitsPool
    {
        private Melee[] _meleePool;

        private int _capacity = 10;

        public UnitsPool(Melee meleePrefab, float meleeDamage, float meleeHealth, float meleeSpeed)
        {
            _meleePool = CreateMelee(meleeDamage, meleeHealth, meleeSpeed, meleePrefab);
        }

        public Melee GetMelee()
        {
            foreach (var melee in _meleePool)
            {
                if (melee.gameObject.activeSelf == false)
                {
                    melee.gameObject.SetActive(true);

                    return melee;
                }
            }

            throw new System.Exception("Not enough MeleeUnit in The pool!!!");
        }

        private Melee[] CreateMelee(float meleeDamage, float meleeHealth, float meleeSpeed, Melee prefab)
        {
            Melee[] pool = new Melee[_capacity];

            for (int i = 0; i < _capacity; i++)
            {
                Melee melee = GameObject.Instantiate(prefab);
                melee.Init(meleeHealth, meleeDamage, meleeSpeed);
                melee.gameObject.SetActive(false);
                pool[i] = melee;
            }

            return pool;
        }
    }
}
