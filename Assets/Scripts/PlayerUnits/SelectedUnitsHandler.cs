using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class SelectedUnitsHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundMask;

        private List<Selectable> _units = new List<Selectable>();
        private List<Selectable> _selectedUnits = new List<Selectable>();

        private ArmyFormation _armyFormation = new ArmyFormation();

        private void OnDisable()
        {
            foreach (Selectable unit in _units)
            {
                unit.Selected -= OnSelect;
                unit.Deselected -= OnDeselct;
            }
        }

        public void Init(Unit[] units)
        {
            foreach (var unit in units)
            {
                unit.Selected += OnSelect;
                unit.Deselected += OnDeselct;

                _units.Add(unit);
            }
        }

        public void OnSelect(Selectable unit)
        {
            _selectedUnits.Add(unit);
        }

        public void OnDeselct(Selectable unit)
        {
            _selectedUnits.Remove(unit);
        }

        public void MoveUnits(Vector3 position)
        {
            if (_selectedUnits.Count > 0)
            {
                Vector3[] formation = _armyFormation.GetFormationDestination(position, _selectedUnits.Count);

                for (int i = 0; i < _selectedUnits.Count; i++)
                {
                    if (_selectedUnits[i] is Unit unit)
                    {
                        unit.Move(formation[i]);
                    }
                }
            }
        }
    }
}