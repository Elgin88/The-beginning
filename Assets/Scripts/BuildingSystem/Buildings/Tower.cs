using Assets.Scripts.PlayerComponents.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.BuildingSystem.Buildings
{
    internal class Tower : Building
    {

       // [SerializeField] private Bow _bow;
        
        public override bool IsPlayerObject => gameObject;

        //public void Attack()
        //{
        //    if (_bow.CanAttack)
        //    {
        //        // _animator.SetAnimatorAttackTrigger(_currentWeapon);

        //        _bow.Attack();
        //    }
        //}

        //private void Update()
        //{
        //    Attack();
        //}
    }  
}