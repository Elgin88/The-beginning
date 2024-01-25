using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class CameraXZ : MonoBehaviour
    {
        void Update()
        {
            transform.position = new Vector3(TCameraManager.CurrentCamera.transform.position.x, transform.position.y, TCameraManager.CurrentCamera.transform.position.z);
        }
    }
}

