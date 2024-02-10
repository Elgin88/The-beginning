using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enemy;
using UnityEngine;
using TMPro;

public class TestTargetUI : MonoBehaviour
{
    private TMP_Text _text;
    private EnemyNextTargetFinder _enemyNextTargetFinder;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        _enemyNextTargetFinder = FindAnyObjectByType<EnemyNextTargetFinder>();
    }

    private void Update()
    {
    }
}