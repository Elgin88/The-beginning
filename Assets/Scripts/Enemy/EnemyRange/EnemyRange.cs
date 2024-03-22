using UnityEngine;

namespace Assets.Scripts.EnemyNamespace
{
    internal abstract class EnemyRange : Enemy
    {
        internal abstract void EnableArrow(Vector3 position);
    }
}