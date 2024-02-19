using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Enemy;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerComponents.Weapons;
using Assets.Scripts.PlayerComponents.Weapons.Bows;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class TowerAttack : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Arrow _arrowPrefab;
    [SerializeField] private float _delayOfShoot;
    [SerializeField] private float _damage;
    [SerializeField] private LayerMask _targetlayerMask;

    private bool _isTargetNear;
    private List<Transform> _targets = new();
    private Transform _target;
    private ArrowsPool _poolOfArrows;
    private Coroutine _shootCoroutine;
    private float _currentDelay;

    private void Awake()
    { 
         _poolOfArrows = new ArrowsPool(_arrowPrefab, _damage, _targetlayerMask);
    }

    //private void Attack(Transform target)
    //{
    //    if (_shootCoroutine != null)
    //    {
    //        StopCoroutine(_shootCoroutine);
    //    }

    //    _shootCoroutine = StartCoroutine(Shoot(target));
    //}

    private void Update()
    {
        if (_targets.Count > 0)
        {
            TryForShoot();
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    int mask = 1 << other.gameObject.layer;

    //    _isTargetNear = true;
    //    Debug.Log(other.gameObject.name);

    //    if (other.gameObject.TryGetComponent(out IDamageable target) && mask == _targetlayerMask)
    //    {
    //        _targets.Add(target.Transform);


    //        Attack(target.Transform);
    //    }
    //}


    private void TryForShoot()
    {  
        if(_currentDelay >= _delayOfShoot)
        {  
            Shoot();
            _currentDelay = 0;
        }  
        _currentDelay += Time.deltaTime;

    }

    private void Shoot()
    {        
        for(int i = 0; i < _targets.Count; i++)
            {
                Arrow arrow = _poolOfArrows.GetArrow();

                arrow.transform.position = _shootPoint.position;
                arrow.Fly(_targets[i]);
            }   
    }

    private void OnTriggerEnter(Collider other)
    {
        int mask = 1 << other.gameObject.layer;

        //_isTargetNear = true;
        //Debug.Log(other.gameObject.name);

        if (other.gameObject.TryGetComponent(out IDamageable target) && mask == _targetlayerMask)
        {
            _targets.Add(target.Transform);


           // Attack(target.Transform);
        }
    }

    private void SpotTargets(Transform target)
    {
        _targets.Add(target);
    }


    private void OnTriggerExit(Collider other)
    {

        int mask = 1 << other.gameObject.layer;

        if (other.gameObject.TryGetComponent(out IDamageable target) && mask == _targetlayerMask)
        {
            _targets.Remove(target.Transform);
           // _isTargetNear = false;
        }


        //int mask = 1 << other.gameObject.layer;

        //if (other.gameObject.TryGetComponent(out IDamageable target) && mask == _targetlayerMask)
        //{
        //    _targets.Remove(target.Transform);
        //    _isTargetNear = false;
        //    StopCoroutine(_shootCoroutine);
        //}

    }

    //private IEnumerator Shoot(Transform target)
    //{
    //    while (_isTargetNear)
    //    {
    //        Arrow arrow = _poolOfArrows.GetArrow();

    //        arrow.transform.position = _shootPoint.position;
    //        arrow.Fly(target);
    //        yield return new WaitForSeconds(_delayOfShoot);
    //    }  
    //}
}
