using System;
using UnityEngine;

namespace Assets.Scripts.GameLogic.Utilities
{
    internal class WorldPointFinder
    {
        private Ray _ray;
        private float _rayDistance = 100f;
        private LayerMask _groundMask;

        public WorldPointFinder(LayerMask ground)
        {
            _groundMask = ground;
        }

        public Vector3 GetPosition(Vector3 mousePosition)
        {
            _ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(_ray, out RaycastHit hit, _rayDistance, _groundMask))
            {
                return hit.point;
            }

            throw new Exception("cant find position");
        }

        public Vector3 GetPosition(Vector2 touchPosition)
        {
            _ray = Camera.main.ScreenPointToRay(touchPosition);

            if (Physics.Raycast(_ray, out RaycastHit hit, _rayDistance, _groundMask))
            {
                return hit.point;
            }

            throw new Exception("cant find position");
        }
    }
}
