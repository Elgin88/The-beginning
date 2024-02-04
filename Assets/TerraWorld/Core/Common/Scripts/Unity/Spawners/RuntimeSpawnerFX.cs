using UnityEngine;

namespace TerraUnity.Runtime
{
    //[ExecuteAlways]
    public class RuntimeSpawnerFX : MonoBehaviour
    {
        public bool placeAroundCamera = true;
        public GameObject player;
        public GameObject prefab;

        //TODO: Bug: When activated, settings cannot be changed!
        public bool checkSceneVolumes = false;
        public LayerMask volumeLayers;

        public int count = 3;
        public float lifeTime = 3f;
        public float spawnDelay = 3f;
        [Range(0, 4)] public float randomTimeRange = 2f;
        public float radius = 20f;
        public float startHeight = 0f;
        public float endHeight = 2000f;
        public LayerMask layerMask;
        public float heightOffset = 0.5f;
        public bool getGroundAngle = true;
        public int seedNo = 12345;
        public enum WaterDetection { bypassWater, underWater, onWater }
        public WaterDetection waterDetection = WaterDetection.bypassWater;

        private float checkingHeight = 100000f;
        private GameObject[] effect;
        private GameObject effects;
        private ParticleSystem[] particleSystems;
        private ParticleSystem.EmissionModule[] emissionModules;
        private bool isInsideVolume = false;
        private bool isBypassWater;
        private bool isUnderwater;

        private void OnValidate()
        {
            if (placeAroundCamera)
                player = Camera.main.gameObject;
        }

        void Start()
        {
            if (player == null) return;
            
            Random.InitState(seedNo);
            if (waterDetection == WaterDetection.bypassWater) isBypassWater = true;
            else isBypassWater = false;
            if (waterDetection == WaterDetection.underWater) isUnderwater = true;
            else isUnderwater = false;

            effects = new GameObject("Runtime FX");
            if (TTerraWorldManager.MainTerrainGO != null) effects.transform.parent = TTerraWorldManager.MainTerrainGO.transform;
            effect = new GameObject[count];
            particleSystems = new ParticleSystem[count];
            emissionModules = new ParticleSystem.EmissionModule[count];

            for (int i = 0; i < count; i++)
            {
                effect[i] = Instantiate(prefab, effects.transform);
                effect[i].name = "Effect_" + (i + 1).ToString();
                particleSystems[i] = effect[i].GetComponent<ParticleSystem>();
                emissionModules[i] = particleSystems[i].emission;

                emissionModules[i].enabled = false;
                particleSystems[i].Simulate(1);
                particleSystems[i].Play();
            }

            InvokeRepeating("SpawnEffects", 0, lifeTime + spawnDelay + randomTimeRange);
        }

        private void SpawnEffects()
        {
            if (player == null) return;
            if (checkSceneVolumes && !isInsideVolume) return;

            if (player.transform.position.y >= startHeight && player.transform.position.y <= endHeight)
            {
                if (!Application.isPlaying)
                {
                    Physics.autoSimulation = false;
                    Physics.Simulate(Time.fixedDeltaTime);
                }

                for (int i = 0; i < count; i++)
                {
                    if (effect[i] == null) continue;

                    Vector3 origin = player.transform.position;
                    origin += Random.insideUnitSphere * radius;
                    origin.y = checkingHeight;
                    Ray ray = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    if (Raycasts.RaycastNonAllocSorted(ray, isBypassWater, isUnderwater, out hit, layerMask, Mathf.Infinity, QueryTriggerInteraction.Ignore))
                    {
                        Vector3 position = hit.point;

                        if (position.y >= startHeight && position.y <= endHeight)
                        {
                            position.y += heightOffset;
                            effect[i].transform.position = position;

                            Vector3 normal = hit.normal;

                            if (getGroundAngle)
                                effect[i].transform.rotation = Quaternion.LookRotation(normal);
                        }
                    }
                }

                if (!Application.isPlaying)
                    Physics.autoSimulation = true;

                Invoke("TurnOnEffects", Random.Range(0, randomTimeRange));
                Invoke("TurnOffEffects", lifeTime + Random.Range(0, randomTimeRange));
            }
        }

        private void TurnOnEffects()
        {
            for (int i = 0; i < count; i++)
                emissionModules[i].enabled = true;
        }

        private void TurnOffEffects()
        {
            for (int i = 0; i < count; i++)
                emissionModules[i].enabled = false;
        }

        private void Update()
        {
            if (player == null || !checkSceneVolumes) return;
            transform.position = player.transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (player == null || !checkSceneVolumes) return;

            if (volumeLayers == (volumeLayers | (1 << other.gameObject.layer)))
                isInsideVolume = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (player == null || !checkSceneVolumes) return;

            if (volumeLayers == (volumeLayers | (1 << other.gameObject.layer)))
                isInsideVolume = false;
        }
    }
}

