using Assets.Scripts.UnitStateMachine;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NextTargetFinder))]

    internal class Enemy : MonoBehaviour
    {
    }
}