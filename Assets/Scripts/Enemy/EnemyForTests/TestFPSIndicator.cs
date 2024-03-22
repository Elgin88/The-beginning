using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestFPSIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text _fpsIndicator;

    private void Start()
    {
        StartCoroutine(ShowFPS());
    }

    private IEnumerator ShowFPS()
    {
        while (true)
        {
            _fpsIndicator.text = (1 / Time.deltaTime).ToString();

            yield return new WaitForSeconds(0.15f);
        }
    }
}