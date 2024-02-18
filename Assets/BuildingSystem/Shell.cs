using Assets.Scripts.GameLogic.Damageable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Shell : MonoBehaviour
{
   // [SerializeField] private ParticleSystem _hitEffect;
    [SerializeField] private float _speed;

    private float _damage;
    private Transform _target;

    //private void Awake(Transform target)
    //{
    //    _target = target;
    //}

    void Update()
    {
        Shoot();
    }

    public void Init(Transform target)
    {
        _target = target;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);


        if (other.gameObject.TryGetComponent<IDamageable>(out IDamageable target))
        {
            target.TakeDamage(_damage);
            Destroy(this.gameObject);
            Debug.Log("Должен был уничтожиться");


        }

       // ParticleSystem hitEffect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
       // Destroy(hitEffect.gameObject, 1f);
       
    }

    public void Shoot()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.deltaTime);
    }
}
