using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.PlayerComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.BuildingSystem.New
{
    internal class BuidingSpot : MonoBehaviour
    {
       //// [SerializeField] private int _id;
       
        
        
       // [SerializeField] private float _yOffset;

       // private SpriteRenderer _spriteRenderer;
       // private Transform _newSpotToPlace;

       // public bool IsOccupied;




       // private void Awake()
       // {
       //     _spriteRenderer = GetComponent<SpriteRenderer>();   
       // }

       // private void OnEnable()
       // {
       //     NewBuiding.Destroyed += FreeSpotToBuild;
       // }

       // private void OnDisable()
       // {
       //     NewBuiding.Destroyed -= FreeSpotToBuild;
       // }
       
       // private void FreeSpotToBuild(Transform buidingTransform)
       // {
       //     if(IsOccupied == true && buidingTransform.position == _spotToPlace.position)
       //     {
       //         IsOccupied = false;
       //     }
       // }


       // private void OnTriggerEnter(Collider other)
       // {
       //     if (other.gameObject.TryGetComponent(out Player player))
       //     {
       //         if (_spotToPlace != null && IsOccupied == false)
       //         {
       //             _newSpotToPlace = _spotToPlace;
       //             _newSpotToPlace.position = new Vector3(_spotToPlace.position.x, _spotToPlace.position.y + _yOffset, _spotToPlace.position.z);
       //             Instantiate(_buildPrefab, _spotToPlace);
       //             IsOccupied = true;
       //         }      
       //     }
       // }

       // private void RenderIcon()
       // {
       //     _spriteRenderer.sprite = _iconOfBuilding;
       //    // _spriteRenderer.flipZ = true;
       // }
    }
}