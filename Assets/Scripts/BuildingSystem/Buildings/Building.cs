using System;
using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;

public abstract class Building : MonoBehaviour, IDamageable
{
    public int Cost;
    public float Strength;
    public ParticleSystem EffectOfDestroying;
    public GameObject PrafabOfruins;

    public Action OnDestroyed;

    public void TakeDamage(float damage)
    {
       if(Strength > 0 && damage > 0)
        {
            Strength -= damage;

            if(Strength <= 0)
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
