using UnityEngine;

namespace TerraUnity.Runtime
{
    public class SimpleFollow : MonoBehaviour
    {
        private GameObject _target;
        private float _moveSpeed = 1f;
        private float _offset = 1f;

        private void Update()
        {
            SetFollower(_target, _moveSpeed, _offset);
        }

        public void SetFollower(GameObject target, float moveSpeed, float offset)
        {
            if (target != null)
            {
                _target = target;
                _moveSpeed = moveSpeed;
                _offset = offset;
                transform.position = Vector3.Lerp(transform.position, target.transform.position - (transform.forward * offset), Time.deltaTime * moveSpeed);
            }
            else
            {
                if (GetComponent<BendGrassWhenEnabled>() != null) GrassBendingManager.RemoveBender(GetComponent<BendGrassWhenEnabled>());
                //this.gameObject.SetActive(false);
                Destroy(this.gameObject);
            }
        }
    }
}

