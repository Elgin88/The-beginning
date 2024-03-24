using Assets.Scripts.PlayerComponents;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.AuxiliaryComponents
{
    public class TargetFollow : MonoBehaviour
    {
       // private Player _player;

         [SerializeField] private Transform _target;
        [SerializeField] private float _offsetPositionY;
        [SerializeField] private float _offsetPositionX;
        [SerializeField] private float _offsetPositionZ;


        private void LateUpdate()
        {
            transform.position = new Vector3(_target.position.x + _offsetPositionX,
               _target.position.y + _offsetPositionY, _target.position.z + _offsetPositionZ);
        }

       
    }
}