using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class UnitsPool
    {
        private Unit[] _unitsPool;

        private int _capacity = 10;

        public Unit[] MeleePool => _unitsPool;

        public UnitsPool(UnitData data)
        {
            _unitsPool = CreateUnitsPool(data);
        }

        public Unit GetUnit()
        {
            foreach (var melee in _unitsPool)
            {
                if (melee.gameObject.activeSelf == false)
                {
                    melee.gameObject.SetActive(true);

                    return melee;
                }
            }

            throw new System.Exception("Not enough MeleeUnit in The pool!!!");
        }

        private Unit[] CreateUnitsPool(UnitData data)
        {
            Unit[] pool = new Unit[_capacity];

            for (int i = 0; i < _capacity; i++)
            {
                Unit unit = GameObject.Instantiate(data.Prefab);
                unit.Init(data);
                unit.gameObject.SetActive(false);
                pool[i] = unit;
            }

            return pool;
        }
    }
}
