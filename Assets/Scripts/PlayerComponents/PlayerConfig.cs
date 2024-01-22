using UnityEngine;

namespace Assets.Scripts.PlayerComponents
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
    internal class PlayerConfig : ScriptableObject
    {
        [field: SerializeField, Range(0, 10)] public float Speed {  get; private set; }
        [field: SerializeField, Range(0, 10)] public float RotationSpeed {  get; private set; }
        [field: SerializeField, Range(0, 5)] public float RecoverTime {  get; private set; }
        [field: SerializeField, Range(0, 100)] public float Health {  get; private set; }
    }
}
