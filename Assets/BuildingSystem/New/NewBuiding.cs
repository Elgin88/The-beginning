using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using System;

namespace Assets.BuildingSystem.New
{
    internal abstract class NewBuiding : MonoBehaviour, IDamageable
    {
        public int Cost;
        public float Strength;
       // public ParticleSystem EffectOfDestroying;
        public int Id;

        public Transform Transform => transform;
        public static Action<int> Destroyed;

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
            DestroyImmediate(gameObject);                                              
            Destroyed?.Invoke(Id);
        }
    }
}
