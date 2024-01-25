using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class TCameraFrustumActivation : MonoBehaviour
    {
        private void OnBecameVisible()
        {
            //gameObject.SetActive(true);
            Debug.Log("In View");
        }

        private void OnBecameInvisible()
        {
            //gameObject.SetActive(false);
            Debug.Log("Out of View");
        }
    }
}

