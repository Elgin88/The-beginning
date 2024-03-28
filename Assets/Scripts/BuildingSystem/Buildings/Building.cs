using System;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents;

namespace Assets.Scripts.BuildingSystem
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]

    internal abstract class Building : MonoBehaviour, IDamageable
    {
        public int IndexOfBuilding;
        public float Strength;
        public Transform Transform => transform;
        public abstract bool IsPlayerObject { get; }
        public bool IsDead => false;

        public static Action<Transform> Destroyed;

        public void TakeDamage(float damage)
        {
            if (Strength > 0 && damage > 0)
            {
                Strength -= damage;

                if (Strength <= 0)
                {
                    Destroy();
                }
            }
        }

        protected void Destroy()
        {
            //Instantiate(EffectOfDestroying, transform.position, Quaternion.identity);
            Destroyed?.Invoke(this.transform);
            DestroyImmediate(gameObject);
        }
    }
}