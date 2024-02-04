using UnityEngine;
using System.Collections.Generic;
using Mewlist.MassiveGrass;

namespace TerraUnity.Runtime
{
    public class GetSceneMaterials : MonoBehaviour
    {
        public static List<Material> SceneMaterialsList()
        {
            List<Material> result = new List<Material>();

            // If Unity version is 2020.1 or newer, then inactive objects can be retrieved too
#if UNITY_2020_1_OR_NEWER
            Terrain[] terrains = FindObjectsOfType<Terrain>(true);
            GameObject[] gameObjects = FindObjectsOfType<GameObject>(true);
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>(true);

            // TerraWorld Specific Materials
            TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>(true);
            MassiveGrass[] grassLayers = FindObjectsOfType<MassiveGrass>(true);
            RuntimeSpawnerGPU[] runtimeSpawnersGPU = FindObjectsOfType<RuntimeSpawnerGPU>(true);
            RuntimeSpawnerGO[] runtimeSpawnersGO = FindObjectsOfType<RuntimeSpawnerGO>(true);
#else
            Terrain[] terrains = FindObjectsOfType<Terrain>();
            GameObject[] gameObjects = FindObjectsOfType<GameObject>();
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();

            // TerraWorld Specific Materials
            TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();
            MassiveGrass[] grassLayers = FindObjectsOfType<MassiveGrass>();
            RuntimeSpawnerGPU[] runtimeSpawnersGPU = FindObjectsOfType<RuntimeSpawnerGPU>();
            RuntimeSpawnerGO[] runtimeSpawnersGO = FindObjectsOfType<RuntimeSpawnerGO>();
#endif

            // Terrains' materials
            foreach (Terrain t in terrains)
            {
                Material terrainMaterial = t.materialTemplate;
                if (terrainMaterial != null) result.Add(t.materialTemplate);
            }

            // GameObjects' materials
            foreach (GameObject g in gameObjects)
            {
                Renderer[] renderers = g.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer r in renderers)
                {
                    if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) continue;

                    foreach (Material m in r.sharedMaterials)
                        if (m != null) result.Add(m);
                }
            }

            // Particle Systems' materials
            foreach (ParticleSystem p in particleSystems)
            {
                ParticleSystemRenderer PSR = p.gameObject.GetComponent<ParticleSystemRenderer>();
                if (PSR != null && PSR.sharedMaterials == null || PSR.sharedMaterials.Length == 0) continue;

                foreach (Material m in PSR.sharedMaterials)
                    if (m != null) result.Add(m);
            }


            // TerraWorld Specific Materials
            //---------------------------------------------------------------------------------------------------------------------------------------------------


            // GPU Layers' materials
            if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
            {
                foreach (TScatterParams g in GPULayers)
                {
                    if (g == null || g.LODsMaterials == null) continue;
                    foreach (TScatterParams.LODMaterials lodMats in g.LODsMaterials)
                    {
                        if (lodMats.subMaterials == null) continue;
                        foreach (Material m in lodMats.subMaterials)
                        {
                            if (m == null) continue;
                            result.Add(m);
                        }
                    }
                }
            }

            // Grass Layers' materials
            foreach (MassiveGrass MG in grassLayers)
            {
                if (MG == null || MG.profiles == null) continue;
                foreach (MassiveGrassProfile profile in MG.profiles)
                {
                    if (profile == null) continue;
                    if (profile.Material == null) continue;
                    Material m = profile.Material;
                    if (m == null) continue;
                    result.Add(m);
                }
            }

            // Runtime Spawner GPU materials
            if (runtimeSpawnersGPU != null && runtimeSpawnersGPU.Length > 0 && runtimeSpawnersGPU[0] != null)
            {
                foreach (RuntimeSpawnerGPU g in runtimeSpawnersGPU)
                {
                    if (g == null || g.LODsMaterials == null || g.LODsMaterials.Length == 0) continue;

                    foreach (RuntimeSpawnerGPU.LODMaterials lodMats in g.LODsMaterials)
                    {
                        foreach (Material m in lodMats.subMaterials)
                        {
                            if (m == null) continue;
                            result.Add(m);
                        }
                    }
                }
            }

            // Runtime Spawner GameObject materials
            if (runtimeSpawnersGO != null && runtimeSpawnersGO.Length > 0 && runtimeSpawnersGO[0] != null)
            {
                foreach (RuntimeSpawnerGO g in runtimeSpawnersGO)
                {
                    List<Material> lodMats = new List<Material>();

                    foreach (Renderer r in g.prefab.GetComponentsInChildren<Renderer>(true))
                        lodMats.AddRange(r.sharedMaterials);

                    if (lodMats != null && lodMats.Count > 0)
                    {
                        foreach (Material m in lodMats)
                        {
                            if (m == null) continue;
                            result.Add(m);
                        }
                    }
                }
            }

            return result;
        }
    }
}

