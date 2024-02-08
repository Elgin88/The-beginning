using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class SelectedUnitsHandler
    {
        private List<Selectable> _units;

        public SelectedUnitsHandler() 
        { 
            _units = new List<Selectable>();
        }

        public void AddUnit(Selectable unit)
        {
            _units.Add(unit);
        }

        public void RemoveUnit(Selectable unit)
        {
            _units.Remove(unit);
        }

        public void MoveUnits(Vector3 position)
        {
            foreach (Selectable selectable in _units)
            {
                if (selectable.TryGetComponent<Unit>(out Unit unit))
                {
                    unit.Move(position);
                }
            }
        }
    }
}