using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using System;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.BuildingSystem.Buildings;

namespace Assets.BuildingSystem.New
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshCollider))]
    internal abstract class NewBuiding : MonoBehaviour, IDamageable
    {
        public int Cost;
        public float Strength;
        public float TimeWhilePassable = 10;
        public float CurrentTimeWhilePassable = 0;

       // public ParticleSystem EffectOfDestroying;
       // public int Id;

        public Transform Transform => transform;
        public static Action<Transform> Destroyed;
        public Rigidbody Rigidbody { get; private set; }
        public MeshCollider MeshCollider { get; private set; }

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

        public void Start()
        {
           // StartCoroutine(BecomePassable());
        }


        public void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            MeshCollider = GetComponent<MeshCollider>();
        }

        //public void Init(Transform spotToInit)
        //{
        //    Instantiate(this, spotToInit);
        //}

        public IEnumerator BecomePassable()
        {
            while (TimeWhilePassable > CurrentTimeWhilePassable)
            {
                MeshCollider.isTrigger = true;
                CurrentTimeWhilePassable += Time.deltaTime;
            }

            yield return null;
        }



        protected void Destroy()
        {
            //Instantiate(EffectOfDestroying, transform.position, Quaternion.identity);
            DestroyImmediate(gameObject);                                              
            Destroyed?.Invoke(Transform);
        }
    }
}
