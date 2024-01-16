using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal abstract class Building : MonoBehaviour, IDamageable
    {
        public int Cost;
        public int Strength;
        public ParticleSystem EffectOfDestroying;
        public GameObject PrafabOfruins;

        public Action OnDestroyed;

        public Transform Transform => transform;

        public void TakeDamage(int damage)
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
            Destroy(gameObject);                                                           // ������ ����� ������������, �� �� ��� ����� ����� ���������� ���������,
            Instantiate(PrafabOfruins, transform.position, Quaternion.identity);    // ����� ��������� �� ���� ����� ������, �� ���� ����� ������ ����� ������ ���������� �� ������
            OnDestroyed?.Invoke();
        }
    }
}