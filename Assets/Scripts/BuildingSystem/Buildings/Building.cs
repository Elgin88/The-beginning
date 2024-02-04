using System;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal abstract class Building : MonoBehaviour, IDamageable
    {
        public int Cost;
        public float Strength;
        public ParticleSystem EffectOfDestroying;
        public GameObject PrafabOfruins;

        public Action OnDestroyed;

        public Transform Transform => transform;

        public abstract bool IsPlayerObject { get; }

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
            Instantiate(EffectOfDestroying, transform.position, Quaternion.identity);
            Destroy(gameObject);                                                           // здание будет уничтожаться, но на его месте будут оставаться развалины,
            Instantiate(PrafabOfruins, transform.position, Quaternion.identity);    // чтобы построить на этом месте заного, их надо будет снести через панель управления за деньги
            OnDestroyed?.Invoke();
        }
    }
}