using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    public class EnemyRangeArrow : MonoBehaviour
    {
        [SerializeField] private TestStart _testStart;
        [SerializeField] private TestFinish _testFinish;
        [SerializeField] private float _speedOfMove;
        [SerializeField] private float _speedOfRotation;

        private void Awake()
        {
            transform.DOMove(_testFinish.transform.position, _speedOfMove).SetSpeedBased().SetEase(Ease.Linear);
        }
    }
}