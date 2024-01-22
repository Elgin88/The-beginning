using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Service
{
    [CreateAssetMenu(menuName = "BuildingSystem/BuildingsContainer")]
    internal class BuildingsContainer : ScriptableObject
    {
        public List<BuildingInfo> BuildingInformation;
    }

    [Serializable]
    public class BuildingInfo
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public Vector2Int Size { get; private set; } = Vector2Int.one;
        [field: SerializeField] public GameObject Prefab { get; private set; }
    }
}