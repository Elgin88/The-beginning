using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class ArmyFormation
    {
        private float _unitSpacing = 2f;
        private float _offset = 2f;

        public Vector3[] GetFormationDestination(Vector3 centerPosition, int numUnits)
        {
            List<Vector3> destinations = new List<Vector3>();

            int rows = Mathf.CeilToInt(Mathf.Sqrt(numUnits));
            int cols = Mathf.CeilToInt((float)numUnits / rows);

            Vector3 startPos = centerPosition - new Vector3((cols - 1) * _unitSpacing / _offset, 0, (rows - 1) * _unitSpacing / _offset);

            for (int i = 0; i < numUnits; i++)
            {
                int row = i / cols;
                int col = i % cols;

                Vector3 dest = startPos + new Vector3(col * _unitSpacing, 0, row * _unitSpacing);
                destinations.Add(dest);
            }

            return destinations.ToArray();
        }
    }
}
