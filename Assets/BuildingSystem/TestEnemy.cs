using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamageable
{
    public bool IsPlayerObject => false;

    public float Health = 20;

    public bool IsDead => throw new System.NotImplementedException();

    public Transform Transform => transform;

    public void TakeDamage(float damage)
    {
        if (Health > 0 && damage > 0)
        {
            Health -= damage;

            if (Health <= 0)
            {
                Destroy();
            }
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);                                                       
    }
}
