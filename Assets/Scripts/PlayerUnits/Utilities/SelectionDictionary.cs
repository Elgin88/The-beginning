using System.Collections.Generic;

namespace Assets.Scripts.PlayerUnits.Utilities
{
    internal class SelectionDictionary
    {
        private Dictionary<int, Unit> _selectedTable = new Dictionary<int, Unit>();

        public void AddSelected(Unit unit)
        {
            int id = unit.GetInstanceID();

            if (_selectedTable.ContainsKey(id) == false)
            {
                _selectedTable.Add(id, unit);
                unit.Select();
            }
        }

        public void DeselectByID(int id)
        {
            if (_selectedTable.ContainsKey(id))
            {
                _selectedTable[id].Deselect();
                _selectedTable.Remove(id);
            }
        }

        public void DeselectAll()
        {
            if (_selectedTable.Count > 0)
            {
                foreach (KeyValuePair<int, Unit> unit in _selectedTable)
                {
                    _selectedTable[unit.Key].Deselect();
                }

                _selectedTable.Clear();
            }
        }
    }
}
