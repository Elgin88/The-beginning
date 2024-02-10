using Assets.Scripts.Enemy;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerComponents.Weapons;
using Assets.Scripts.PlayerComponents.Weapons.Bows;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TowerAttack : MonoBehaviour
{
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Arrow _arrowPrefab;
    [SerializeField] private float _delayOfShoot;
    [SerializeField] private bool _isPlayerTower;
    [SerializeField] private float _damage;

    private List<IDamageable> _targets = new();
    private IDamageable _target;
    private float _timeBetweenShoots = 5;
    private ArrowsPool _poolOfArrows;

    private Coroutine _shootCoroutine;

    private void Awake()
    {
        _poolOfArrows = new ArrowsPool(_arrowPrefab, _damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPlayerTower)
        {
            if (other.gameObject.TryGetComponent(out IDamageable idamageable))
            {
                if (!idamageable.IsPlayerObject)
                {
                    StartCoroutine(Shoot(idamageable.Transform));
                }
            }
        }
       
        if (!_isPlayerTower)
        {
            if (other.gameObject.TryGetComponent(out IDamageable idamageable))
            {
                if (idamageable.IsPlayerObject)
                {
                    StartCoroutine(Shoot(idamageable.Transform));
                }
            }
        }
    }

    private IEnumerator Shoot(Transform target)
    {
        yield return new WaitForSeconds(_delayOfShoot);
        Arrow arrow = _poolOfArrows.GetArrow();

        arrow.transform.position = _shootPoint.position;


       // Arrow arrow = Instantiate(_arrowPrefab);
       // arrow.gameObject.SetActive(true);
       // arrow.transform.position = _shootPoint.position;
       // arrow.Init(_damage);
        arrow.Fly(target);
       
        //yield return null;
    }

    private void TryToShot(Transform target)
    {
        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
        }
       
        _shootCoroutine = StartCoroutine(Shoot(target));
    }
}
