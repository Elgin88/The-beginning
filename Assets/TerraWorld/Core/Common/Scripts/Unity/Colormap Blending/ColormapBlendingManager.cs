using UnityEngine;
using System.Collections.Generic;
using Mewlist.MassiveGrass;

namespace TerraUnity.Runtime
{
    public struct ColormapBlendingParams
    {
        public bool hasColormapBlendingGPU;
        public bool hasColormapBlendingGrass;
        public bool hasColormapBlendingRuntimeSpawners;
        public float blendingStrengthGPU;
        public float blendingStrengthGrass;
        public float blendingStrengthRuntimeSpawner;
        public bool excludeOpaqueMaterialsGPU;
        public bool excludeOpaqueMaterialsGrass;
        public bool excludeOpaqueMaterialsRuntimeSpawner;
    }

    [ExecuteAlways]
    public class ColormapBlendingManager : MonoBehaviour
    {
        private static ColormapBlendingManager Script { get => GetScript(); }
        private static ColormapBlendingManager _script;

        private static ColormapBlendingManager GetScript()
        {
            if (_script == null)
            {
                ColormapBlendingManager script = TTerraWorldManager.SceneSettingsGO1.GetComponent<ColormapBlendingManager>();

                if (script != null)
                    _script = script;
                else
                    _script = TTerraWorldManager.SceneSettingsGO1.AddComponent<ColormapBlendingManager>();
            }

            return _script;
        }

        public bool hasColormapBlendingGPU = true;
        public bool hasColormapBlendingGrass = true;
        public bool hasColormapBlendingRuntimeSpawners = true;
        [Range(1f, 100f)] public float blendingStrengthGPU = 50f;
        [Range(1f, 100f)] public float blendingStrengthGrass = 50f;
        [Range(1f, 100f)] public float blendingStrengthRuntimeSpawner = 50f;
        public bool excludeOpaqueMaterialsGPU = false;
        public bool excludeOpaqueMaterialsGrass = false;
        public bool excludeOpaqueMaterialsRuntimeSpawner = false;

        private const float strengthDivision = 40f;

        private static ColormapBlendingParams _parameters;

        public static ColormapBlendingParams GetParams()
        {
            _parameters.hasColormapBlendingGPU = Script.hasColormapBlendingGPU;
            _parameters.hasColormapBlendingGrass = Script.hasColormapBlendingGrass;
            _parameters.hasColormapBlendingRuntimeSpawners = Script.hasColormapBlendingRuntimeSpawners;
            _parameters.blendingStrengthGPU = Script.blendingStrengthGPU;
            _parameters.blendingStrengthGrass = Script.blendingStrengthGrass;
            _parameters.blendingStrengthRuntimeSpawner = Script.blendingStrengthRuntimeSpawner;
            _parameters.excludeOpaqueMaterialsGPU = Script.excludeOpaqueMaterialsGPU;
            _parameters.excludeOpaqueMaterialsGrass = Script.excludeOpaqueMaterialsGrass;
            _parameters.excludeOpaqueMaterialsRuntimeSpawner = Script.excludeOpaqueMaterialsRuntimeSpawner;

            return _parameters;
        }

        public static void SetParams(ColormapBlendingParams parameters)
        {
            _parameters = parameters;

            Script.hasColormapBlendingGPU = parameters.hasColormapBlendingGPU;
            Script.hasColormapBlendingGrass = parameters.hasColormapBlendingGrass;
            Script.hasColormapBlendingRuntimeSpawners = parameters.hasColormapBlendingRuntimeSpawners;
            Script.blendingStrengthGPU = parameters.blendingStrengthGPU;
            Script.blendingStrengthGrass = parameters.blendingStrengthGrass;
            Script.blendingStrengthRuntimeSpawner = parameters.blendingStrengthRuntimeSpawner;
            Script.excludeOpaqueMaterialsGPU = parameters.excludeOpaqueMaterialsGPU;
            Script.excludeOpaqueMaterialsGrass = parameters.excludeOpaqueMaterialsGrass;
            Script.excludeOpaqueMaterialsRuntimeSpawner = parameters.excludeOpaqueMaterialsRuntimeSpawner;

            Script.Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        private void Apply()
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;
            SetupColormapBlendingGPU();
            SetupColormapBlendingGrass();
            SetupColormapBlendingRuntimeSpawners();
        }

        private void SetupColormapBlendingGPU()
        {
            TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();

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

                            if (m.shader == Shader.Find("TerraUnity/Standard") || m.shader == Shader.Find("TerraUnity/Standard Tessellation"))
                            {
                                if (hasColormapBlendingGPU)
                                {
                                    if (Colormap != null)
                                    {
                                        // Check if material's rendering mode is not opaque and then apply Colormap Blending
                                        if (excludeOpaqueMaterialsGPU)
                                        {
                                            if (m.HasProperty("_Mode") && m.GetFloat("_Mode") != 0)
                                            {
                                                m.SetTexture("_Colormap", Colormap);
                                                m.SetFloat("_ColormapBlending", 1);
                                                m.SetFloat("_ColormapInfluence", blendingStrengthGPU / strengthDivision);
                                                m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                                m.EnableKeyword("_COLORMAP_BLENDING");
                                            }
                                            else
                                            {
                                                m.SetFloat("_ColormapBlending", 0);
                                                m.DisableKeyword("_COLORMAP_BLENDING");
                                            }
                                        }
                                        else
                                        {
                                            m.SetTexture("_Colormap", Colormap);
                                            m.SetFloat("_ColormapBlending", 1);
                                            m.SetFloat("_ColormapInfluence", blendingStrengthGPU / strengthDivision);
                                            m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                            m.EnableKeyword("_COLORMAP_BLENDING");
                                        }
                                    }
                                }
                                else
                                {
                                    m.SetFloat("_ColormapBlending", 0);
                                    m.DisableKeyword("_COLORMAP_BLENDING");
                                }

                                //InternalEditorUtility.SetIsInspectorExpanded(m, true);
                                //ActiveEditorTracker.sharedTracker.ForceRebuild();
                                //SetStandardMaterialParams.SwitchMaterialBlendingType(m, (SetStandardMaterialParams.BlendMode)m.GetFloat("_Mode"));
                            }
                        }
                    }
                }
            }
        }

        private void SetupColormapBlendingGrass()
        {
            foreach (MassiveGrass MG in FindObjectsOfType<MassiveGrass>())
            {
                if (MG == null || MG.profiles == null) continue;
                foreach (MassiveGrassProfile profile in MG.profiles)
                {
                    if (profile == null) continue;
                    if (profile.Material == null) continue;
                    Material m = profile.Material;

                    if (m != null)
                    {
                        if (m.shader == Shader.Find("TerraUnity/Standard") || m.shader == Shader.Find("TerraUnity/Standard Tessellation"))
                        {
                            if (hasColormapBlendingGrass)
                            {
                                if (Colormap != null)
                                {
                                    // Check if material's rendering mode is not opaque and then apply Colormap Blending
                                    if (excludeOpaqueMaterialsGrass)
                                    {
                                        if (m.HasProperty("_Mode") && m.GetFloat("_Mode") != 0)
                                        {
                                            m.SetTexture("_Colormap", Colormap);
                                            m.SetFloat("_ColormapBlending", 1);
                                            m.SetFloat("_ColormapInfluence", blendingStrengthGrass / strengthDivision);
                                            m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                            m.EnableKeyword("_COLORMAP_BLENDING");
                                        }
                                        else
                                        {
                                            m.SetFloat("_ColormapBlending", 0);
                                            m.DisableKeyword("_COLORMAP_BLENDING");
                                        }
                                    }
                                    else
                                    {
                                        m.SetTexture("_Colormap", Colormap);
                                        m.SetFloat("_ColormapBlending", 1);
                                        m.SetFloat("_ColormapInfluence", blendingStrengthGrass / strengthDivision);
                                        m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                        m.EnableKeyword("_COLORMAP_BLENDING");
                                    }
                                }
                            }
                            else
                            {
                                m.SetFloat("_ColormapBlending", 0);
                                m.DisableKeyword("_COLORMAP_BLENDING");
                            }
                        }
                    }
                }
            }
        }

        private void SetupColormapBlendingRuntimeSpawners()
        {
            RuntimeSpawnerGPU[] runtimeSpawnersGPU = FindObjectsOfType<RuntimeSpawnerGPU>();

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

                            if (m.shader == Shader.Find("TerraUnity/Standard") || m.shader == Shader.Find("TerraUnity/Standard Tessellation"))
                            {
                                if (hasColormapBlendingRuntimeSpawners)
                                {
                                    if (Colormap != null)
                                    {
                                        // Check if material's rendering mode is not opaque and then apply Colormap Blending
                                        if (excludeOpaqueMaterialsRuntimeSpawner)
                                        {
                                            if (m.HasProperty("_Mode") && m.GetFloat("_Mode") != 0)
                                            {
                                                m.SetTexture("_Colormap", Colormap);
                                                m.SetFloat("_ColormapBlending", 1);
                                                m.SetFloat("_ColormapInfluence", blendingStrengthRuntimeSpawner / strengthDivision);
                                                m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                                m.EnableKeyword("_COLORMAP_BLENDING");
                                            }
                                            else
                                            {
                                                m.SetFloat("_ColormapBlending", 0);
                                                m.DisableKeyword("_COLORMAP_BLENDING");
                                            }
                                        }
                                        else
                                        {
                                            m.SetTexture("_Colormap", Colormap);
                                            m.SetFloat("_ColormapBlending", 1);
                                            m.SetFloat("_ColormapInfluence", blendingStrengthRuntimeSpawner / strengthDivision);
                                            m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                            m.EnableKeyword("_COLORMAP_BLENDING");
                                        }
                                    }
                                }
                                else
                                {
                                    m.SetFloat("_ColormapBlending", 0);
                                    m.DisableKeyword("_COLORMAP_BLENDING");
                                }
                            }
                        }
                    }
                }
            }

            RuntimeSpawnerGO[] runtimeSpawnersGO = FindObjectsOfType<RuntimeSpawnerGO>();

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

                            if (m.shader == Shader.Find("TerraUnity/Standard") || m.shader == Shader.Find("TerraUnity/Standard Tessellation"))
                            {
                                if (hasColormapBlendingRuntimeSpawners)
                                {
                                    if (Colormap != null)
                                    {
                                        // Check if material's rendering mode is not opaque and then apply Colormap Blending
                                        if (excludeOpaqueMaterialsRuntimeSpawner)
                                        {
                                            if (m.HasProperty("_Mode") && m.GetFloat("_Mode") != 0)
                                            {
                                                m.SetTexture("_Colormap", Colormap);
                                                m.SetFloat("_ColormapBlending", 1);
                                                m.SetFloat("_ColormapInfluence", blendingStrengthRuntimeSpawner / strengthDivision);
                                                m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                                m.EnableKeyword("_COLORMAP_BLENDING");
                                            }
                                            else
                                            {
                                                m.SetFloat("_ColormapBlending", 0);
                                                m.DisableKeyword("_COLORMAP_BLENDING");
                                            }
                                        }
                                        else
                                        {
                                            m.SetTexture("_Colormap", Colormap);
                                            m.SetFloat("_ColormapBlending", 1);
                                            m.SetFloat("_ColormapInfluence", blendingStrengthRuntimeSpawner / strengthDivision);
                                            m.SetVector("_WorldSize", new Vector4(TTerraWorldManager.MainTerrain.terrainData.size.x, TTerraWorldManager.MainTerrain.terrainData.size.z, 0, 0));
                                            m.EnableKeyword("_COLORMAP_BLENDING");
                                        }
                                    }
                                }
                                else
                                {
                                    m.SetFloat("_ColormapBlending", 0);
                                    m.DisableKeyword("_COLORMAP_BLENDING");
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Texture2D Colormap { get => GetColormap(); }
        private static Texture2D _colormap = null;

        private static Texture2D GetColormap()
        {
            if (_colormap != null) return _colormap;

#if UNITY_EDITOR && TERRAWORLD_PRO
            _colormap = UnityEditor.AssetDatabase.LoadAssetAtPath(TTerraWorldManager.WorkDirectoryLocalPath + "ColorMap.jpg", typeof(Texture2D)) as Texture2D;
#endif
            return _colormap;
        }

        //public static Color32[,] TerrainColors { get => GetTerrainColors(); }
        //private static Color32[,] _terrainColors = null;

        //        private static Color32[,] GetTerrainColors ()
        //        {
        //            if (_terrainColors != null && _terrainColors.Length > 0) return _terrainColors;
        //
        //#if UNITY_EDITOR
        //            if (Colormap != null)
        //            {
        //                TextureImporter imageImport = AssetImporter.GetAtPath(WorkDirectoryLocalPath + "ColorMap.jpg") as TextureImporter;
        //                imageImport.isReadable = true;
        //                AssetDatabase.ImportAsset(WorkDirectoryLocalPath + "ColorMap.jpg", ImportAssetOptions.ForceUpdate);
        //                AssetDatabase.Refresh();
        //                Color32[] _terrainColors1D = Colormap.GetPixels32();
        //                _terrainColors = new Color32[Colormap.width, Colormap.height];
        //
        //                for (int i = 0; i < Colormap.height; i++)
        //                    for (int j = 0; j < Colormap.width; j++)
        //                        _terrainColors[i, j] = _terrainColors1D[i * Colormap.width + j];
        //            }
        //            else
        //                _terrainColors = null;
        //#endif
        //            return _terrainColors;
        //
        //        }
    }
}

