using System;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.BuildingSystem
{
    [RequireComponent(typeof(Rigidbody))]

    internal abstract class Building : MonoBehaviour, IDamageable
    {
        public float Strength;
        //public ParticleSystem EffectOfDestroying;
       
       // public int Cost;

        public static Action<Transform> Destroyed;
       // public Action<int> Created;
        
        //public void Start()
        //{
        //    Created?.Invoke(Cost);
        //}

        public int IndexOfBuilding;

        public Transform Transform => transform;

        public abstract bool IsPlayerObject { get; }

        public bool IsDead => false;

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