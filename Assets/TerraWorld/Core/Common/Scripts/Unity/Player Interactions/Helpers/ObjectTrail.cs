using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    public static class Utility
    {
        public static void Invoke(this MonoBehaviour mb, Action f, float delay)
        {
            mb.StartCoroutine(InvokeRoutine(f, delay));
        }

        private static IEnumerator InvokeRoutine(Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            f();
        }
    }

    public class ObjectTrail : MonoBehaviour
    {
        public int cloneCount = 16;
        public float lifeTimeInSeconds = 5f;
        public float deadZoneMeters = 1f;
        public float followSpeed = 1f;

        private List<GameObject> clones;
        private Vector3 cachedPosition = Vector3.zero;
        private int counter = 0;
        private List<SimpleFollow> followers;

        private void Start()
        {
            clones = new List<GameObject>();
            followers = new List<SimpleFollow>();

            for (int i = 0; i < cloneCount; i++)
            {
                GameObject go = Instantiate(this.gameObject);
                go.transform.position = Vector3.zero;
                go.AddComponent<SimpleFollow>();
                followers.Add(go.GetComponent<SimpleFollow>());
                go.hideFlags = HideFlags.HideAndDontSave;

                if (go.GetComponent<MeshRenderer>() != null)
                    go.GetComponent<MeshRenderer>().enabled = false;

                //if (go.GetComponent<Rigidbody>() != null)
                //    Destroy(go.GetComponent<Rigidbody>());
                //
                //if (go.GetComponent<Collider>() != null)
                //    Destroy(go.GetComponent<Collider>());

                if (go.GetComponent<TimedDestructions>() != null)
                    go.GetComponent<TimedDestructions>().enabled = false;

                if (go.GetComponent<ObjectTrail>() != null)
                    go.GetComponent<ObjectTrail>().enabled = false;

                go.SetActive(false);

                clones.Add(go);
            }
        }

        private void Update()
        {
            Vector3 position = transform.position;

            if (IsOutsideDeadZone(cachedPosition, position, deadZoneMeters) && counter < cloneCount)
            {
                GameObject clone = clones[counter];

                if (!clone.activeSelf)
                {
                    if (counter == 0)
                    {
                        clone.transform.position = position + (transform.forward * -deadZoneMeters);
                        followers[0].SetFollower(gameObject, followSpeed, deadZoneMeters);
                    }
                    else
                    {
                        clone.transform.position = clones[counter - 1].transform.position + (clones[counter - 1].transform.forward * -deadZoneMeters);
                        followers[counter].SetFollower(clones[counter - 1], followSpeed, deadZoneMeters);
                    }

                    clone.SetActive(true);
                    //this.Invoke(() => DeactivateObject(clone), lifeTimeInSeconds);
                    counter++;
                }

                cachedPosition = position;
            }
        }

        private bool IsOutsideDeadZone(Vector3 cachedPosition, Vector3 currentPosition, float deadZone)
        {
            if
            (
                Mathf.Abs(cachedPosition.x - currentPosition.x) > deadZone &&
                Mathf.Abs(cachedPosition.z - currentPosition.z) > deadZone
            )
                return true;

            return false;
        }

        private void DeactivateObject (GameObject clone)
        {
            clone.SetActive(false);
            counter--;
        }
    }
}

