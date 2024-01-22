using UnityEngine;

namespace Assets.Scripts.BuildingSystem.States
{
    internal interface IBuildingState
    {
        public void EndState();
        public void OnAction(Vector3Int gridPosition);
        public void UpdateState(Vector3Int gridPosition);
    }
}