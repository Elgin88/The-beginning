using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal abstract class EnemyRange : Enemy
    {
        internal abstract void EnableArrow(Vector3 position);
    }
}