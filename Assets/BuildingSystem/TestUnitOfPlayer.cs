using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnitOfPlayer : MonoBehaviour, IDamageable
{
    public bool IsPlayerObject => true;

    public bool IsDead => throw new System.NotImplementedException();
    
    public float Health = 20;

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
