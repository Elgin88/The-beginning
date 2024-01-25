using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(BendGrassWhenEnabled))]
    public class Rotors : MonoBehaviour
    {
        [Range(0.1f, 100f)]public float rotorSpeed = 40f;
        [Range(0.1f, 50f)] public float rotorSize = 30f;
        private BendGrassWhenEnabled grassBend;

        private void OnEnable()
        {
            Init();
        }

        private void Start()
        {
            Init();
        }

        void Update()
        {
            RunRotors();
        }

        private void Init ()
        {
            grassBend = GetComponent<BendGrassWhenEnabled>();
        }

        private void RunRotors ()
        {
            if (grassBend == null) return;
            //transform.localScale = new Vector3(rotorSize * 2f, 0.2f, 1);
            //transform.Rotate(Vector3.up, rotorSpeed);
            grassBend.BendRadius = rotorSize;
            grassBend.BendIntensity = rotorSpeed / 100f;
            //helicopterWind.speed = rotorSpeed / 100f;
            //helicopterWind.bendingRange = rotorSpeed / 100f;
        }
    }
}

