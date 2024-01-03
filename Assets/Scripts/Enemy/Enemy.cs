using UnityEngine;
using Assets.Scripts.UnitStateMachine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NextTargetFinder))]

    internal class Enemy : MonoBehaviour
    {
    }
}