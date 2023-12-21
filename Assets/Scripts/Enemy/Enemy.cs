using UnityEngine;
using Scripts.UnitStateMachine;

namespace Scripts.Enemy
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(NextTargetFinder))]

    public class Enemy : MonoBehaviour
    {
    }
}