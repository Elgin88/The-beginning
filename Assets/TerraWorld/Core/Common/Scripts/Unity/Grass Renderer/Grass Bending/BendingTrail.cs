using UnityEngine;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    [RequireComponent(typeof(TrailRenderer))]
    public class BendingTrail : MonoBehaviour
    {
        [Range(1, 15)] public int maximumTrailsCount = 15;
        public float lifeTimeInSeconds = 5f;
        public float trailsDistanceInMeters = 1f;
        public float adoptionSpeed = 1f;
        public bool debugSpheres = false;

        private TrailRenderer trailRenderer;
        private Vector3[] positions;
        private List<GameObject> clones;
        private GameObject clone;
        private GameObject trailsParent;

        private void Start()
        {
            trailRenderer = GetComponent<TrailRenderer>();
            trailRenderer.time = lifeTimeInSeconds;
            trailRenderer.minVertexDistance = trailsDistanceInMeters;
            positions = new Vector3[128];

            trailsParent = new GameObject("Trails");
            clones = new List<GameObject>();

            for (int i = 0; i < maximumTrailsCount; i++)
            {
                if (!debugSpheres)
                    clone = new GameObject("Trail " + (i + 1));
                else
                {
                    clone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    clone.name = "Trail " + (i + 1);

                    if (clone.GetComponent<SphereCollider>() != null)
                        clone.GetComponent<SphereCollider>().enabled = false;
                }

                clone.transform.position = Vector3.zero;
                clone.transform.parent = trailsParent.transform;
                BendGrassWhenEnabled BGWE_Original = this.gameObject.GetComponent<BendGrassWhenEnabled>();
                BendGrassWhenEnabled BGWE_New = clone.AddComponent<BendGrassWhenEnabled>();

                float fade = Mathf.InverseLerp(0, maximumTrailsCount - 1, i);
                //float fadeRadius = Mathf.Clamp(BGWE_Original.BendRadius * fade, 1f, 1000f) + (BGWE_Original.BendRadius / 2f);
                float fadeRadius = (BGWE_Original.BendRadius * fade) + (BGWE_Original.BendRadius / 4f);

                BGWE_New.BendRadius = fadeRadius;

                BGWE_New.BendIntensity = BGWE_Original.BendIntensity / 2f;
                BGWE_New.Priority = BGWE_Original.Priority;
                clones.Add(clone);
            }
        }

        private void LateUpdate()
        {
            int trailsCount = trailRenderer.GetPositions(positions);

            //for (int i = trailsCount - 1; i >= 0; i--)
            for (int i = 0; i < maximumTrailsCount; i++)
            {
                if (i < trailsCount)
                {
                    if (i == maximumTrailsCount - 1)
                        clones[i].transform.position = Vector3.Lerp(clones[i].transform.position, transform.position, Time.deltaTime * adoptionSpeed);
                    else
                    {
                        clones[i].transform.position = Vector3.Lerp(clones[i].transform.position, positions[i], Time.deltaTime * adoptionSpeed);

                        //int index = Mathf.Clamp(i - 1, 0, trailsCount);
                        //clones[i].transform.position = Vector3.Lerp(clones[i].transform.position, clones[index].transform.position, Time.deltaTime * adoptionSpeed);
                    }

                    if (Vector3.Distance(clones[i].transform.position, positions[i]) < trailsDistanceInMeters)
                        clones[i].SetActive(true);
                }   
                else
                    clones[i].SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (trailsParent != null)
                Destroy(trailsParent);
        }
    }
}

