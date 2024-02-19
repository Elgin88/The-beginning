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
    //[SerializeField] private Shell _shellPrefab;
    [SerializeField] private float _delayOfShoot;
    [SerializeField] private bool _isPlayerTower;
    [SerializeField] private float _damage;
    [SerializeField] private LayerMask _targetlayerMask;

    private bool _canAttack;

    private List<IDamageable> _targets = new();
    private IDamageable _target;
    private float _timeBetweenShoots = 5;
    private ArrowsPool _poolOfArrows;

    private string _enemy = "Enemy";
    private string _playerUnit = "PlayerUnit";
    private string _currentLayerMask;

    private Coroutine _shootCoroutine;

    private void Awake()
    { 
         _poolOfArrows = new ArrowsPool(_arrowPrefab, _damage, _targetlayerMask);

        _canAttack = true;
    }

    private void SetCurrentLayerMask(IDamageable idamageable)
    {
        if (idamageable.IsPlayerObject)
        {
            _currentLayerMask = _playerUnit;
        }
        else
        {
            _currentLayerMask = _enemy;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
            if (other.gameObject.TryGetComponent(out IDamageable idamageable) && _canAttack == true)
            {
                SetCurrentLayerMask(idamageable);

                if (_isPlayerTower && _currentLayerMask == _enemy)
                {
                    StartCoroutine(Shoot(idamageable.Transform));
                    //InitShell(idamageable);
                    Debug.Log("Лечу на врага");
                }

                if (!_isPlayerTower && _currentLayerMask == _playerUnit)
                {
                    StartCoroutine(Shoot(idamageable.Transform));
                    // InitShell(idamageable);
                     Debug.Log("Лечу на игрока");

                 }
                
               // StartCoroutine(Shoot(idamageable.Transform));
            }
    }

    //private void InitShell(IDamageable idamageable)
    //{
    //   Shell shell =  Instantiate(_shellPrefab, _shootPoint.position, Quaternion.identity);
    //   shell.Init(idamageable.Transform);
    //}

    //private IEnumerator InitShell2(IDamageable idamageable)
    //{
    //    yield return new WaitForSeconds(_delayOfShoot);

    //    Shell shell = Instantiate(_shellPrefab, _shootPoint.position, Quaternion.identity);
    //    shell.Init(idamageable.Transform);
    //}




    private IEnumerator Shoot(Transform target)
    {
        _canAttack = false;

        Arrow arrow = _poolOfArrows.GetArrow();

        arrow.transform.position = _shootPoint.position;


        // Arrow arrow = Instantiate(_arrowPrefab);
        // arrow.gameObject.SetActive(true);
        // arrow.transform.position = _shootPoint.position;
        // arrow.Init(_damage);
        arrow.Fly(target);

        yield return new WaitForSeconds(_delayOfShoot);

        _canAttack = true;
    }

    //private void TryToShot(Transform target)
    //{
    //    if (_shootCoroutine != null)
    //    {
    //        StopCoroutine(_shootCoroutine);
    //    }

    //    _shootCoroutine = StartCoroutine(Shoot(target));
    //}
}
