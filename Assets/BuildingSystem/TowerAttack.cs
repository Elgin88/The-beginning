using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Enemy;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerComponents.Weapons;
using Assets.Scripts.PlayerComponents.Weapons.Bows;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private List<Transform> _targets = new();
    private Transform _target;
    private ArrowsPool _poolOfArrows;
    private float _currentDelay;

    private void Awake()
    { 
         _poolOfArrows = new ArrowsPool(_arrowPrefab, _damage, _targetlayerMask);
    }

    private void Update()
    {
        if (_targets.Count > 0)
        {
            TryForShoot();
            CleanTargets(_targets);
        }
    }

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
        //for (int i = 0; i < _targets.Count; i++)   //AllTargetsAttack
        //{
        //    Arrow arrow = _poolOfArrows.GetArrow();

        //    arrow.transform.position = _shootPoint.position;
        //    arrow.Fly(_targets[i]);
        //}

       //FirstTargetAttack
        Arrow arrow = _poolOfArrows.GetArrow();
        arrow.transform.position = _shootPoint.position;
        arrow.Fly(_targets.First());
    }

    private void CleanTargets(List<Transform> targets)
    {
        for(int i = 0; i < _targets.Count; i++)
        {
            if (targets[i] == null && targets[i].gameObject.activeSelf == false)
            {
                _targets.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int mask = 1 << other.gameObject.layer;

        if (other.gameObject.TryGetComponent(out IDamageable target) && mask == _targetlayerMask)
        {
            _targets.Add(target.Transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int mask = 1 << other.gameObject.layer;

        if (other.gameObject.TryGetComponent(out IDamageable target) && mask == _targetlayerMask)
        {
            _targets.Remove(target.Transform);
        }
    }
}
