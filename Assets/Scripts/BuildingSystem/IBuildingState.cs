using UnityEngine;

public interface IBuildingState
{
    public void EndState();
    public void OnAction(Vector3Int gridPosition);
    public void UpdateState(Vector3Int gridPosition);
}