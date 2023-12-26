using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Button _closeButton;
    [SerializeField] private GameObject _buildPanel;
    //[SerializeField] private GameObject _buildSystem;


    public static Action PanelActivated;
    public static Action PanelDeActivated;


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
        _buildPanel.SetActive(true);
        PanelActivated?.Invoke();
       // _buildSystem.SetActive(true);
    }

    public void OnCloseButtonClicked()
    {
        _buildPanel.SetActive(false);
        PanelDeActivated?.Invoke();
       // _buildSystem.SetActive(false);
    }
}
