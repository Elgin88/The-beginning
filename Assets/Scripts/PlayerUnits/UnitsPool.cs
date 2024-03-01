using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class UnitsPool
    {
        private Unit[] _meleePool;
        private SelectedUnitsHandler _handler;

        private int _capacity = 10;

        public Unit[] MeleePool => _meleePool;

        public UnitsPool(Unit meleePrefab, ParticleSystem ring, float meleeHealth, LayerMask mask, SelectedUnitsHandler handler)
        {
            _handler = handler;
            _meleePool = CreateMelee(meleeHealth, meleePrefab, ring, mask);
        }

        public Unit GetMelee()
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

        private Unit[] CreateMelee(float meleeHealth, Unit prefab, ParticleSystem ring, LayerMask mask)
        {
            Unit[] pool = new Unit[_capacity];

            for (int i = 0; i < _capacity; i++)
            {
                Unit melee = GameObject.Instantiate(prefab);
                melee.InitUnit(meleeHealth, mask);
                melee.Init(ring, _handler);
                melee.gameObject.SetActive(false);
                pool[i] = melee;
            }

            return pool;
        }
    }
}
