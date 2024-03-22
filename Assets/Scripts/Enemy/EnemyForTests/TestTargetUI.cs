using Assets.Scripts.EnemyNamespace;
using TMPro;
using UnityEngine;

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