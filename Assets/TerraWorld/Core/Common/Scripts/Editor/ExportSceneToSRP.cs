#if (TERRAWORLD_PRO || TERRAWORLD_LITE)
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
//using System.Diagnostics;
using System.IO;
using Mewlist.MassiveGrass;
using TerraUnity.Runtime;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public class ExportSceneToSRP : Editor
    {
        //[MenuItem("Tools/TerraUnity/Export Scene To SRP (Experimental)", false, 20)]
        static void Init()
        {
            //if (EditorUtility.DisplayDialog("", "\"WARNING\"\n", "Cancel", "Continue")) return;
            if (EditorUtility.DisplayDialog("HDRP/URP SCENE EXPORT", "This will automatically export a unitypackage out of your current scene with Unity's default shaders and settings to be usable in any HDRP/URP project templates!", "Cancel", "Continue")) return;
            Application.OpenURL("https://terraunity.com/community/topic/convert-terraworld-scenes-from-standard-rendering-pipeline-to-hdrp-urp/");
            ExportSceneToUnityPackage();
        }

        static string scenePath = null;
        static Terrain[] terrains;
        static List<string> terrainMaterials;
        //static List<Renderer> GPURenderers;
        static List<string> GPUMaterials;
        static List<MassiveGrassProfile> grassProfiles;
        static List<string> grassMaterials;
        static List<string> gameObjectMaterials;
        static List<ParticleSystemRenderer> particleRenderers;
        static List<string> particleMaterials;
        static List<Renderer> goRenderers;
        static List<string> waterMaterials;
        static bool disableSceneSettings = false;
        static GameObject sceneSettingsGO = null;
        static bool sceneSettingsActivation;

        static bool crepuscularIsEnabled;

#if UNITY_STANDALONE_WIN
        static bool atmosphericScatteringIsEnabled;
        static bool atmosphericScatteringSunIsEnabled;
        static bool atmosphericScatteringDeferredIsEnabled;
        static bool volumetricFogIsEnabled;
#endif

        static WaterBase[] waterBase;
        static bool waterBaseIsEnabled;
        static SpecularLighting[] specularLighting;
        static bool specularLightingIsEnabled;
        static PlanarReflection[] planarReflection;
        static bool planarReflectionIsEnabled;
        static GerstnerDisplace[] gerstnerDisplace;
        static bool gerstnerDisplaceIsEnabled;
        static GetWaterPlaneHeight[] getWaterPlaneHeight;
        static bool getWaterPlaneHeightIsEnabled;

        private static void ExportSceneToUnityPackage ()
        {
            EditorUtility.DisplayProgressBar("SCENE EXPORT IN PROGRESS", "Warming up scene elements to export", 0.2f);
            WarmUpScene();

            // Save scene if needed and export UnityPackage from the scene file for importing into any SRP projects
            scenePath = null;

            try
            {
                scenePath = Path.GetFullPath(SceneManager.GetActiveScene().path);
            }
            catch
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Application.dataPath + "/TerraWorld_Scene.unity");
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.DisplayProgressBar("SCENE EXPORT IN PROGRESS", "Collecting assets to create unitypackage", 0.4f);
                ExportScene();

                EditorUtility.DisplayProgressBar("SCENE EXPORT IN PROGRESS", "Reverting back scene elements to their original state", 0.8f);
                RevertScene();

                EditorUtility.DisplayProgressBar("SCENE EXPORT IN PROGRESS", "Success", 1f);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Scene Export Success!", "Scene package has been successfully exported in the project root!\n\nYou can now Import it in any SRP project templates!", "OK"); // \n\nNOTE: You should exclude folder => TerraWorld/Graphics/Pro/Shaders from the package list window while importing into a SRP project for faster import times!
            }
        }

        private static void WarmUpScene()
        {
            // Change terrain materials to Unity Standard if TerraFormer used
            terrains = FindObjectsOfType(typeof(Terrain)) as Terrain[];
            terrainMaterials = new List<string>();

            foreach (Terrain t in terrains)
            {
                Material terrainMaterial = t.materialTemplate;

                if (terrainMaterial != null)
                {
                    terrainMaterials.Add(t.materialTemplate.shader.name);

                    if (terrainMaterial.shader == Shader.Find("TerraUnity/TerraFormer") || terrainMaterial.shader == Shader.Find("TerraUnity/TerraFormer Instanced"))
                        t.materialTemplate.shader = Shader.Find("Nature/Terrain/Standard");
                }
            }

            // Change GPU Instance layer materials to Unity Standard if Terra-Standard is used
            TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();
            //GPURenderers = new List<Renderer>();
            GPUMaterials = new List<string>();

            if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
            {
                foreach (TScatterParams g in GPULayers)
                {
                    Renderer[] renderers = g.Prefab.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer r in renderers)
                    {
                        if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) continue;

                        foreach (Material m in r.sharedMaterials)
                        {
                            if (m == null) continue;

                            //GPURenderers.Add(r);
                            GPUMaterials.Add(m.shader.name);
                            //UnityEngine.Debug.Log(m.shader.name);

                            if (m.shader == Shader.Find("TerraUnity/Standard") || m.shader == Shader.Find("TerraUnity/Standard Tessellation"))
                                m.shader = Shader.Find("Standard");
                        }
                    }
                }
            }

            // Change Grass layer materials to Unity Standard if Terra-Standard is used
            List<GrassLayer> GrassLayers = new List<GrassLayer>();
            grassProfiles = new List<MassiveGrassProfile>();
            grassMaterials = new List<string>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                    if (go.GetComponent<GrassLayer>() != null)
                        GrassLayers.Add(go.GetComponent<GrassLayer>());

            if (GrassLayers != null && GrassLayers.Count > 0 && GrassLayers[0] != null)
            {
                for (int i = 0; i < GrassLayers.Count; i++)
                {
                    if (GrassLayers[i].MGP.Material == null) continue;

                    grassProfiles.Add(GrassLayers[i].MGP);
                    grassMaterials.Add(GrassLayers[i].MGP.Material.shader.name);

                    if (GrassLayers[i].MGP.Material.shader == Shader.Find("TerraUnity/Standard") || GrassLayers[i].MGP.Material.shader == Shader.Find("TerraUnity/Standard Tessellation"))
                        GrassLayers[i].MGP.Material.shader = Shader.Find("Standard");
                }
            }

            // Change GameObject/TerraMesh layer materials to Unity Standard if Terra-Standard is used
            GameObject[] gameObjects = FindObjectsOfType<GameObject>();
            gameObjectMaterials = new List<string>();

            foreach (GameObject g in gameObjects)
            {
                Renderer[] renderers = g.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer r in renderers)
                {
                    if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) continue;

                    foreach (Material m in r.sharedMaterials)
                    {
                        if (m == null) continue;

                        gameObjectMaterials.Add(m.shader.name);

                        if (m.shader == Shader.Find("TerraUnity/Standard") || m.shader == Shader.Find("TerraUnity/Standard Tessellation"))
                            m.shader = Shader.Find("Standard");
                    }
                }
            }

            // Change Cloud materials to Unity Standard Prticles if Terra-Clouds is used
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
            particleRenderers = new List<ParticleSystemRenderer>();
            particleMaterials = new List<string>();

            foreach (ParticleSystem p in particleSystems)
            {
                ParticleSystemRenderer cloudRenderer = p.gameObject.GetComponent<ParticleSystemRenderer>();

                if (cloudRenderer != null && cloudRenderer.sharedMaterial != null)
                {
                    particleRenderers.Add(cloudRenderer);
                    particleMaterials.Add(cloudRenderer.sharedMaterial.shader.name);

                    if (cloudRenderer.sharedMaterial.shader == Shader.Find("TerraUnity/Clouds") || cloudRenderer.sharedMaterial.shader == Shader.Find("Particles/Standard Surface"))
                    {
                        //cloudRenderer.sharedMaterial.shader = Shader.Find("Particles/Standard Surface");
                        cloudRenderer.sharedMaterial.shader = Shader.Find("Standard");
                    }
                }
            }

            // Change Water materials to Unity Standard if Terra-Water is used
            GameObject[] sceneObjects = FindObjectsOfType<GameObject>();
            goRenderers = new List<Renderer>();
            waterMaterials = new List<string>();

            foreach (GameObject g in sceneObjects)
            {
                Renderer renderer = g.GetComponent<Renderer>();

                if (renderer != null && renderer.sharedMaterial != null)
                {
                    goRenderers.Add(renderer);
                    waterMaterials.Add(renderer.sharedMaterial.shader.name);

                    if (renderer.sharedMaterial.shader == Shader.Find("TerraUnity/Water4") || renderer.sharedMaterial.shader == Shader.Find("TerraUnity/SimpleWater4"))
                        renderer.sharedMaterial.shader = Shader.Find("Standard");
                }
            }

            // Disable Water Scripts
            waterBase = FindObjectsOfType<WaterBase>();
            specularLighting = FindObjectsOfType<SpecularLighting>();
            planarReflection = FindObjectsOfType<PlanarReflection>();
            gerstnerDisplace = FindObjectsOfType<GerstnerDisplace>();
            getWaterPlaneHeight = FindObjectsOfType<GetWaterPlaneHeight>();

            foreach (WaterBase a in waterBase)
            {
                waterBaseIsEnabled = a.enabled;
                a.enabled = false;
                break;
            }

            foreach (SpecularLighting a in specularLighting)
            {
                specularLightingIsEnabled = a.enabled;
                a.enabled = false;
                break;
            }

            foreach (PlanarReflection a in planarReflection)
            {
                planarReflectionIsEnabled = a.enabled;
                a.enabled = false;
                break;
            }

            foreach (GerstnerDisplace a in gerstnerDisplace)
            {
                gerstnerDisplaceIsEnabled = a.enabled;
                a.enabled = false;
                break;
            }

            foreach (GetWaterPlaneHeight a in getWaterPlaneHeight)
            {
                getWaterPlaneHeightIsEnabled = a.enabled;
                a.enabled = false;
                break;
            }

            // Disable "Scene Settings" GameObject if existing
            if (disableSceneSettings)
            {
                sceneSettingsGO = null;
                Transform[] transforms = FindObjectsOfType(typeof(Transform)) as Transform[];

                foreach (Transform t in transforms)
                {
                    if (t != null && t.name == "Scene Settings" && t.parent.GetComponent<TTerraWorldManager>() != null)
                    {
                        sceneSettingsGO = t.gameObject;
                        sceneSettingsActivation = sceneSettingsGO.activeSelf;
                        sceneSettingsGO.SetActive(false);
                    }
                }
            }

            // Disable Visual FX
            DisableVisualFX();
        }

        private static void ExportScene()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.Refresh();

            // Hide Shaders directory from project before making the package so that it won't be imported into SRP projects
            string shadersPath = Path.GetFullPath("Assets/TerraWorld/Graphics/Pro/Shaders");
            if (Directory.Exists(shadersPath)) Directory.Move(shadersPath, shadersPath.Replace("Shaders", ".Shaders"));

            scenePath = SceneManager.GetActiveScene().path;
            List<string> exportedPackageAssetList = new List<string>();
            exportedPackageAssetList.Add("Assets/mcs.rsp");
            exportedPackageAssetList.Add("Assets/csc.rsp");
            exportedPackageAssetList.Add("Assets/TerraWorld/Core");
            exportedPackageAssetList.Add("Assets/TerraWorld/Graphics");
            exportedPackageAssetList.Add("Assets/TerraWorld/Plugins");
            exportedPackageAssetList.Add(scenePath);
            string exportPath = TAddresses.projectPath;
            string exportFileName = exportPath + "SRP Scene.unitypackage";
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), exportFileName, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies); // | ExportPackageOptions.Interactive

            //TODO: Check if the following line is working properly!
            EditorUtility.RevealInFinder(exportPath);
            //Process.Start(Path.GetFullPath(exportPath));
        }

        private static void RevertScene ()
        {
            // Reveal Shaders directory from project which was previously being hidden before package importing
            string shadersPath = Path.GetFullPath("Assets/TerraWorld/Graphics/Pro/.Shaders");
            if (Directory.Exists(shadersPath)) Directory.Move(shadersPath, shadersPath.Replace(".Shaders", "Shaders"));

            try
            {
                // Revert back terrain materials to their original
                for (int i = 0; i < terrains.Length; i++)
                terrains[i].materialTemplate.shader = Shader.Find(terrainMaterials[i]);
            }
            catch { }

            try
            {
                // Revert back GPU layer materials to their original
                TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();
                int index = 0;

                if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                {
                    foreach (TScatterParams g in GPULayers)
                    {
                        Renderer[] renderers = g.Prefab.GetComponentsInChildren<Renderer>(true);

                        foreach (Renderer r in renderers)
                        {
                            if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) continue;

                            foreach (Material m in r.sharedMaterials)
                            {
                                if (m == null) continue;
                                m.shader = Shader.Find(GPUMaterials[index]);

                                //UnityEngine.Debug.Log(GPUMaterials[index]);
                                //UnityEngine.Debug.Log(GPUMaterials[index] + "   " + AssetDatabase.GetAssetPath(Shader.Find(GPUMaterials[index])));

                                index++;
                                UpdateMaterial(m);
                            }
                        }
                    }
                }

                //for (int i = 0; i < GPURenderers.Count; i++)
                //GPURenderers[i].sharedMaterial.shader = Shader.Find(GPUMaterials[i]);
            }
            catch { }

            try
            {
                // Revert back Grass layer materials to their original
                for (int i = 0; i < grassProfiles.Count; i++)
                {
                    Material m = grassProfiles[i].Material;
                    m.shader = Shader.Find(grassMaterials[i]);
                    UpdateMaterial(m);
                }
            }
            catch { }

            try
            {
                // Revert back GameObject/TerraMesh layer materials to their original
                GameObject[] gameObjects = FindObjectsOfType<GameObject>();
                int index = 0;

                foreach (GameObject g in gameObjects)
                {
                    Renderer[] renderers = g.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer r in renderers)
                    {
                        if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) continue;

                        foreach (Material m in r.sharedMaterials)
                        {
                            if (m == null) continue;
                            m.shader = Shader.Find(gameObjectMaterials[index]);
                            index++;
                            UpdateMaterial(m);
                        }
                    }
                }
            }
            catch { }

            try
            {
                // Revert back particle materials to their original
                for (int i = 0; i < particleRenderers.Count; i++)
                {
                    Material m = particleRenderers[i].sharedMaterial;
                    m.shader = Shader.Find(particleMaterials[i]);
                    UpdateMaterial(m);
                }
            }
            catch { }

            try
            {
                // Revert back water materials to their original
                for (int i = 0; i < goRenderers.Count; i++)
                {
                    Material m = goRenderers[i].sharedMaterial;
                    m.shader = Shader.Find(waterMaterials[i]);
                    UpdateMaterial(m);
                }
            }
            catch { }

            try
            {
                // Revert back water scripts to their original
                foreach (WaterBase a in waterBase) a.enabled = waterBaseIsEnabled;
                foreach (SpecularLighting a in specularLighting) a.enabled = specularLightingIsEnabled;
                foreach (PlanarReflection a in planarReflection) a.enabled = planarReflectionIsEnabled;
                foreach (GerstnerDisplace a in gerstnerDisplace) a.enabled = gerstnerDisplaceIsEnabled;
                foreach (GetWaterPlaneHeight a in getWaterPlaneHeight) a.enabled = getWaterPlaneHeightIsEnabled;
            }
            catch { }

            try
            {
                // Revert back "Scene Settings" activation to its original
                if (disableSceneSettings)
                    if (sceneSettingsGO != null)
                        sceneSettingsGO.SetActive(sceneSettingsActivation);
            }
            catch { }

            try
            {
                // Revert Visual FX
                RevertVisualFX();
            }
            catch { }

            // Save scene to its initial state
            //EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            //AssetDatabase.Refresh();
        }

        private static void UpdateMaterial (Material m)
        {
            if (m.shader == Shader.Find("TerraUnity/Clouds"))
                m.renderQueue = 3000;

            //if
            //(
            //    (
            //        m.shader == Shader.Find("Standard") ||
            //        m.shader == Shader.Find("Standard (Specular setup)")
            //    )
            //    && m.HasProperty("_Mode")
            //)

            if(m.HasProperty("_Mode"))
                SetStandardMaterialParams.SwitchMaterialBlendingType(m, (SetStandardMaterialParams.BlendMode)m.GetFloat("_Mode"));
        }

        private static void DisableVisualFX()
        {
            crepuscularIsEnabled = Crepuscular.Enabled;
            Crepuscular.Enabled = false;

#if UNITY_STANDALONE_WIN
            //atmosphericScatteringIsEnabled = AtmosphericScattering.Enabled;
            //AtmosphericScattering.Enabled = false;

            //atmosphericScatteringSunIsEnabled = AtmosphericScatteringSun.Enabled;
            //AtmosphericScatteringSun.Enabled = false;

            //atmosphericScatteringDeferredIsEnabled = AtmosphericScatteringDeferred.Enabled;
            //AtmosphericScatteringDeferred.Enabled = false;

            volumetricFogIsEnabled = VolumetricFog.Enabled;
            VolumetricFog.Enabled = false;
#endif
        }

        private static void RevertVisualFX()
        {
            Crepuscular.Enabled = crepuscularIsEnabled;

#if UNITY_STANDALONE_WIN
            //AtmosphericScattering.Enabled = atmosphericScatteringIsEnabled;
            //AtmosphericScatteringSun.Enabled = atmosphericScatteringSunIsEnabled;
            //AtmosphericScatteringDeferred.Enabled = atmosphericScatteringDeferredIsEnabled;
            VolumetricFog.Enabled = volumetricFogIsEnabled;
#endif
        }
    }
#endif
}
#endif
#endif

