using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.PlayerComponents;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace Assets.BuildingSystem.New
{
    [RequireComponent(typeof(SpriteRenderer))]
    internal class BuildPoint : MonoBehaviour
    {
        [SerializeField] private Transform _spotToPlace;
        [SerializeField] private Sprite _iconOfBuilding;
        [SerializeField] private Button _buildButton;
        [SerializeField] private GameObject _buildPrefab;

        private bool _isOccupied;
        private SpriteRenderer _spriteRenderer;

        private void OnEnable()
        {
            _buildButton.onClick.AddListener(Build);
            NewBuiding.Destroyed += FreeSpotToBuild;
        }

        private void OnDisable()
        {
            _buildButton.onClick.RemoveListener(Build);
            NewBuiding.Destroyed -= FreeSpotToBuild;
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
       
        private void Start()
        {
            RenderIcon();
        }
       
        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.gameObject.TryGetComponent(out Player player))
            {
                _buildButton.gameObject.SetActive(true);
            }
        }

        private void FreeSpotToBuild(Transform buidingTransform)
        {
            if (_isOccupied == true && buidingTransform.position == _spotToPlace.position)
            {
                _isOccupied = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.gameObject.TryGetComponent(out Player player))
            {
                _buildButton.gameObject.SetActive(false);     
            }
        }

        private void RenderIcon()
        {
            _spriteRenderer.sprite = _iconOfBuilding;
            
        }

        private void Build()
        {
            if (_spotToPlace != null && _isOccupied == false)
            {
                Instantiate(_buildPrefab, _spotToPlace);
                _isOccupied = true;
            }
        }
    }
}