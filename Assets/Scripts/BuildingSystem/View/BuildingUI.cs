using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.BuildingSystem.View
{
    internal class BuildingUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _buildPanel;

        public static Action PanelActivated;
        public static Action PanelDeActivated;


        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            StartBuildingZone.PlayerWentIn += OnPlayerWentIn;
            StartBuildingZone.PlayerWentOut += OnPlayerWentOut;
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            StartBuildingZone.PlayerWentIn -= OnPlayerWentIn;
            StartBuildingZone.PlayerWentOut -= OnPlayerWentOut;
        }

        public void OnButtonClicked()
        {
            _buildPanel.SetActive(true);
            PanelActivated?.Invoke();
        }

        public void OnCloseButtonClicked()
        {
            _buildPanel.SetActive(false);
            PanelDeActivated?.Invoke();
        }

        private void OnPlayerWentIn()
        {
            _button.gameObject.SetActive(true);
        }

        private void OnPlayerWentOut()
        {
            _button.gameObject.SetActive(false);
        }
    }
}