using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.PlayerComponents;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace Assets.BuildingSystem.New
{
    internal class BuildPoint : MonoBehaviour
    {
        [SerializeField] private Transform _spotToPlace;
        [SerializeField] private GameObject _visualObject;
        [SerializeField] private Button _buildButton;
        [SerializeField] private GameObject _buildPrefab;

        private bool _isOccupied;
        private int speedOfRotate = 200;


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

        private void Update()
        {
            RotateVisualObject();
        }

        private void RotateVisualObject()
        {
            _visualObject.transform.Rotate(0, speedOfRotate * Time.deltaTime, 0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.gameObject.TryGetComponent(out Player player))
            {
                if (_isOccupied == false)
                {
                    _buildButton.gameObject.SetActive(true);
                }   
            }
        }

        private void FreeSpotToBuild(Transform buidingTransform)
        {
            if (_isOccupied == true && buidingTransform.position == _spotToPlace.position)
            {
                _isOccupied = false;
                _visualObject.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.gameObject.TryGetComponent(out Player player))
            {
                if(_buildButton.gameObject.activeSelf == true)
                {
                    _buildButton.gameObject.SetActive(false);
                }      
            }
        }



        private void Build()
        {
            if (_spotToPlace != null && _isOccupied == false)
            {
                Instantiate(_buildPrefab, _spotToPlace);
                _isOccupied = true;
                _visualObject.SetActive(false);
                _buildButton.gameObject.SetActive(false);
            }
        }
    }
}