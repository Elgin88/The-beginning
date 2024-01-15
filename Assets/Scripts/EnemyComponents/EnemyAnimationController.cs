using UnityEngine;

namespace Assets.Scripts.Enemy
{
    internal class EnemyAnimationController : MonoBehaviour
    {

        private Animator _animator;

        internal Animator Animator => _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}