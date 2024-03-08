using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class SelectedUnitsHandler
    {
        private static LayerMask _groundMask = LayerMask.GetMask("Ground");

        private List<Selectable> _units = new List<Selectable>();
        private Ray _ray;
        private float _rayDistance = 40f;
        private Vector3 _mousePosition;

        public void Init(Unit[] units)
        {
            foreach (var unit in units)
            {
                _units.Add(unit);
            }
        }

        public void AddUnit(Selectable unit)
        {
            _units.Add(unit);
        }

        public void RemoveUnit(Selectable unit)
        {
            _units.Remove(unit);
        }

        public void MoveUnits()
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out RaycastHit hit, _rayDistance, _groundMask))
            {
                _mousePosition = hit.point;
            }

            _mousePosition = new Vector3(_mousePosition.x, _mousePosition.y + 1, _mousePosition.z);

            foreach (Unit unit in _units)
            {
                if (unit.IsSelected && unit.gameObject.activeSelf)
                {
                    unit.Move(_mousePosition);
                }
            }
        }

        public bool IsAnyUnitSelected()
        {
            foreach (Unit unit in _units)
            {
                if (unit.IsSelected && unit.gameObject.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }
    }
}