using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class SelectedUnitsHandler
    {
        private static LayerMask _groundMask = LayerMask.GetMask("Water");

        private List<Selectable> _units;
        private Ray _ray;
        private float _rayDistance = 40f;
        private Vector3 _mousePosition;

        public void Init(Unit[] units)
        {
            _units = new List<Selectable>();

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

            Debug.Log("Moving");
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out RaycastHit hit, _rayDistance, _groundMask))
            {
                _mousePosition = hit.point;
                Debug.Log(_mousePosition);
            }

            foreach (Unit unit in _units)
            {
                if (unit.IsSelected && unit.gameObject.activeSelf)
                {
                    unit.Move(_mousePosition);
                }
            }
        }
    }
}