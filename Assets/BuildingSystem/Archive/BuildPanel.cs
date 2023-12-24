using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanel : MonoBehaviour
{
    [SerializeField] private Button _mainBuildingButton;
    [SerializeField] private Button _towerButton;
    [SerializeField] private Button _resourceBuildingButton;
    [SerializeField] private Button _barracksButton;
    [SerializeField] private BuildingButtonsIndexesContainer _buildingButtonsIndexesContainer;
    [SerializeField] private PlacementSystem _buildPlacement;



    //private int _buildIndex;
    //private string _clickedButton;
    
    //private void OnEnable()
    //{
    //    _mainBuildingButton.onClick.AddListener(OnButtonClicked(0));

    //}

    ////private void SubscribeToButtons()
    ////{
    ////    for(int i = 0; i < _buldingButtons.Count; i++)
    ////    {
    ////        _buldingButtons[i].onClick.AddListener(OnButtonClicked(0));
    ////    }
    ////}

    //private void OnButtonClicked(int i) //int buildIndex
    //{
    //    _buildPlacement.StartPlacement(i);
    //}
}
