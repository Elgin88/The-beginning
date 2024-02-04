using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class RealtimeReflectionsManager : MonoBehaviour
    {
        void Update()
        {
            transform.position = TCameraManager.CurrentCamera.transform.position;
        }
    }
}

