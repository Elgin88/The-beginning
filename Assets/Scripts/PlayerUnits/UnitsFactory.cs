using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    internal class UnitsFactory : MonoBehaviour
    {
        [SerializeField] private Melee _meleePrefab;

        private SelectedUnitsHandler _handler;
        private UnitsPool _pool;

        public UnitsFactory() 
        { 
            _handler = new SelectedUnitsHandler();
            _pool = new UnitsPool(_meleePrefab ,1, 2, 3);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {

            }
        }
    }
}
