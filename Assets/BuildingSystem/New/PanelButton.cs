using UnityEngine;
using UnityEngine.UI;

namespace Assets.BuildingSystem.New
{
    internal class PanelButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _panel;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        public void OnButtonClicked()
        {
            _panel.SetActive(true);
        }

        public void OnCloseButtonClicked()
        {
            _panel.SetActive(false);
        }
    }
}