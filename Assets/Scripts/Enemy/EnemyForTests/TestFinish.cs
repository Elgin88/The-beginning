using UnityEngine;

public class TestFinish : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Transform _startFinish;
    [SerializeField] private Transform _stopFinish;

    private bool _isForward = true;

    private void Update()
    {
        if (_isForward)
        {
            transform.position = Vector3.MoveTowards(transform.position, _stopFinish.position, _speed * Time.deltaTime);

            if (transform.position == _stopFinish.position)
            {
                _isForward = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, _startFinish.position, _speed * Time.deltaTime);

            if (transform.position == _startFinish.position)
            {
                _isForward = true;
            }
        }
    }
}