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
        [SerializeField] private StartBuildingZone _startBuildingZone;

        public static Action BuildPanelActivated;
        public static Action BuildPanelDeActivated;


        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _startBuildingZone.PlayerWentIn += OnPlayerWentIn;
            _startBuildingZone.PlayerWentOut += OnPlayerWentOut;
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            _startBuildingZone.PlayerWentIn -= OnPlayerWentIn;
            _startBuildingZone.PlayerWentOut -= OnPlayerWentOut;
        }

        public void OnButtonClicked()
        {
            _buildPanel.SetActive(true);
            BuildPanelActivated?.Invoke();
        }

        public void OnCloseButtonClicked()
        {
            _buildPanel.SetActive(false);
            BuildPanelDeActivated?.Invoke();
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