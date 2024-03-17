using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class SelectedUnitsHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundMask;

        private List<Selectable> _units = new List<Selectable>();
        private List<Selectable> _selectedUnits = new List<Selectable>();

        private Ray _ray;
        private float _rayDistance = 40f;
        private Vector3 _mousePosition;

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

        public void MoveUnits()
        {
            if (_selectedUnits.Count > 0)
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(_ray, out RaycastHit hit, _rayDistance, _groundMask))
                {
                    _mousePosition = hit.point;
                }

                _mousePosition = new Vector3(_mousePosition.x, _mousePosition.y + 1, _mousePosition.z);

                Vector3[] formation = _armyFormation.GetFormationDestination(_mousePosition, _selectedUnits.Count);

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