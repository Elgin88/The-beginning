using UnityEngine;

namespace Assets.Scripts.PlayerUnits
{
    [CreateAssetMenu(fileName = "NewUnit", menuName = "Data/UnitData")]
    internal class UnitData : ScriptableObject
    {
        public string Name;
        public Unit Prefab;
        public LayerMask EnemyLayerMask;
        public float Health;
        public float Speed;
        public float Damage;
        public float AttackSpeed;
        public float AttackRange;
        public float AggroRange;
    }
}
