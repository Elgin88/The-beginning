using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _speed;

    private float _currentSpeed;

    private void Start()
    {
        _currentSpeed = _speed;
    }
}