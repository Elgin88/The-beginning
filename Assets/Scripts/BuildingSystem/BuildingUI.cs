using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.BuildingSystem
{
    internal class BuildingUI : MonoBehaviour
    {
        [SerializeField] private Button _buildButton;

        private string _buttonText = "Построить за ";

        public Action ButtonClicked;

        private void OnEnable()
        {
            _buildButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _buildButton.onClick.RemoveListener(OnButtonClicked);
        }

        public void SetButtonText(int costOfBuilding)
        {
            _buildButton.GetComponentInChildren<TMP_Text>().text = _buttonText + costOfBuilding;
        }

        private void OnButtonClicked()
        {
            ButtonClicked?.Invoke();
        }

        public void ActiveButton()
        {
            _buildButton.gameObject.SetActive(true);
        }

        public void TryToDeActiveButton()
        {
            if (_buildButton.gameObject.activeSelf == true)
            {
                _buildButton.gameObject.SetActive(false);
            }
        }
    }
}