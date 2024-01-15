using UnityEngine;

namespace Assets.Scripts.Enemy
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Enemy))]

    internal class EnemyAnimationController : MonoBehaviour
    {
        private Animator _animator;
        private Enemy _enemy;

        internal Animator Animator => _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _enemy = GetComponent<Enemy>();
        }
    }
}