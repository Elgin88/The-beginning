// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using UnityEngine;

namespace UnityEditor
{
    internal class TerraStandardTessellation_GUI : ShaderGUI
    {
        private enum WorkflowMode
        {
            Specular,
            Metallic,
            Dielectric
        }

        private enum TessWorkflowMode
        {
            Distance,
            EdgeLength,
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        public enum SmoothnessMapChannel
        {
            SpecularMetallicAlpha,
            AlbedoAlpha,
        }

        private static class Styles
        {
            public static GUIContent uvSetLabel = new GUIContent("UV Set");

            public static GUIContent edgelenText = new GUIContent("Edge Length in PX", "Edge Length in PX. Smaller means more detail.");
            public static GUIContent tessFacText = new GUIContent("Tessellation Factor", "Tessellation Factor Up Close");
            public static GUIContent tessMaxText = new GUIContent("Tess / Disp Fade Distance", "Tessellation & Displacement Max Distance");
            public static GUIContent shoadowGenText = new GUIContent("Shadow Caster LOD", "Shadow Caster Tessellation LOD");

            public static GUIContent dispScaleText = new GUIContent("Disp Scale", "Displacement Scale");
            public static GUIContent dispOffsetText = new GUIContent("Disp Offset", "Displacement Offset");
            public static GUIContent phongText = new GUIContent("Phong Smoothing", "Tess Phong Smoothing Factor");
            public static GUIContent tessModeText = new GUIContent("Tessellation Mode", "in = Edge Length or off= Distance Based Tesselattion Mode");

            public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
            public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
            public static GUIContent specularMapText = new GUIContent("Specular", "Specular (RGB) and Smoothness (A)");
            public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
            public static GUIContent smoothnessText = new GUIContent("Smoothness", "Smoothness value");
            public static GUIContent smoothnessScaleText = new GUIContent("Smoothness", "Smoothness scale factor");
            public static GUIContent smoothnessMapChannelText = new GUIContent("Source", "Smoothness texture and channel");
            public static GUIContent highlightsText = new GUIContent("Specular Highlights", "Specular Highlights");
            public static GUIContent reflectionsText = new GUIContent("Reflections", "Glossy Reflections");
            public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
            public static GUIContent heightMapText = new GUIContent("Height Map", "Height Map (G)");
            public static GUIContent occlusionText = new GUIContent("Occlusion", "Occlusion (G)");
            public static GUIContent emissionText = new GUIContent("Color", "Emission (RGB)");
            public static GUIContent detailMaskText = new GUIContent("Detail Mask", "Mask for Secondary Maps (A)");
            public static GUIContent detailAlbedoText = new GUIContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
            public static GUIContent detailNormalMapText = new GUIContent("Normal Map", "Normal Map");

            public static GUIContent snowMapText = new GUIContent("Layer Diffuse", "Albedo (RGB) and Smoothness (A)");

            public static string tessellationText = "Tessellation Settings";
            public static string displacementText = "Displacement Settings";

            public static string primaryMapsText = "Main Maps";
            public static string secondaryMapsText = "Secondary Maps";
            public static string forwardText = "Forward Rendering Options";
            public static string renderingMode = "Rendering Mode";
            public static string advancedText = "Advanced Options";

            public static string snowText = "Procedural Detail Layer";

            public static GUIContent emissiveWarning = new GUIContent("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
            public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
        }

        MaterialProperty tessellation = null;
        MaterialProperty maxdist = null;
        MaterialProperty shadowgen = null;
        MaterialProperty displacement = null;
        MaterialProperty dispoffset = null;
        MaterialProperty phong = null;
        MaterialProperty tessmode = null;

        MaterialProperty blendMode = null;
        MaterialProperty albedoMap = null;
        MaterialProperty albedoColor = null;
        MaterialProperty alphaCutoff = null;
        MaterialProperty specularMap = null;
        MaterialProperty specularColor = null;
        MaterialProperty metallicMap = null;
        MaterialProperty metallic = null;
        MaterialProperty smoothness = null;
        MaterialProperty smoothnessScale = null;
        MaterialProperty smoothnessMapChannel = null;
        MaterialProperty highlights = null;
        MaterialProperty reflections = null;
        MaterialProperty bumpScale = null;
        MaterialProperty bumpMap = null;
        MaterialProperty occlusionStrength = null;
        MaterialProperty occlusionMap = null;
        MaterialProperty heigtMapScale = null;
        MaterialProperty heightMap = null;
        MaterialProperty emissionColorForRendering = null;
        MaterialProperty emissionMap = null;
        MaterialProperty detailMask = null;
        MaterialProperty detailAlbedoMap = null;
        MaterialProperty detailNormalMapScale = null;
        MaterialProperty detailNormalMap = null;
        MaterialProperty uvSetSecondary = null;

        MaterialProperty proceduralSnow = null;
        MaterialProperty proceduralLayer = null;
        MaterialProperty snowMap = null;
        MaterialProperty snowColor = null;
        MaterialProperty snowTiling = null;
        MaterialProperty snowDirection = null;
        MaterialProperty snowLevel = null;
        MaterialProperty snowSmoothness = null;
        MaterialProperty doubleSided = null;

        // Wind params
        MaterialProperty wind = null;
        MaterialProperty isLeaves = null;
        MaterialProperty isBark = null;
        MaterialProperty isGrass = null;
        MaterialProperty baseWindMultipliers = null; // x: main, y: turbulence, z: flutter
        MaterialProperty tumbleStrength = null;
        MaterialProperty tumbleFrequency = null;
        MaterialProperty timeOffset = null;
        MaterialProperty enableLeafTurbulence = null;
        MaterialProperty leafTurbulence = null;
        MaterialProperty edgeFlutterInfluence = null;
        MaterialProperty enableAdvancedEdgeBending = null;
        MaterialProperty advancedEdgeBending = null;

        // Grass Wind
        MaterialProperty windTime = null;
        MaterialProperty windSpeed = null;
        MaterialProperty windBending = null;

        // Flat Shading params
        MaterialProperty flatShading = null;

        // Use UV3 for detailmap to support NatureManufacture art assets
        MaterialProperty detailMapUV3 = null;

        // Colormap Blending
        MaterialProperty colormapBlending = null;
        MaterialProperty colormap = null;
        MaterialProperty colormapInfluence = null;
        MaterialProperty worldSize = null;

        MaterialEditor m_MaterialEditor;
        WorkflowMode m_WorkflowMode = WorkflowMode.Specular;
        TessWorkflowMode m_TessWorkflowMode = TessWorkflowMode.EdgeLength;
        //ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);

        bool m_FirstTimeApply = true;

        public void FindProperties(MaterialProperty[] props)
        {
            tessmode = FindProperty("_TessMode", props);
            tessellation = FindProperty("_Tess", props);
            maxdist = FindProperty("_maxDist", props, false);
            shadowgen = FindProperty("_ShadowLOD", props);

            phong = FindProperty("_Phong", props);
            displacement = FindProperty("_Displacement", props);
            dispoffset = FindProperty("_DispOffset", props);

            blendMode = FindProperty("_Mode", props);
            albedoMap = FindProperty("_MainTex", props);
            albedoColor = FindProperty("_Color", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            specularMap = FindProperty("_SpecGlossMap", props, false);
            specularColor = FindProperty("_SpecColor", props, false);
            metallicMap = FindProperty("_MetallicGlossMap", props, false);
            metallic = FindProperty("_Metallic", props, false);

            if (specularMap != null && specularColor != null)
                m_WorkflowMode = WorkflowMode.Specular;
            else if (metallicMap != null && metallic != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            else
                m_WorkflowMode = WorkflowMode.Dielectric;
            smoothness = FindProperty("_Glossiness", props);
            smoothnessScale = FindProperty("_GlossMapScale", props, false);
            smoothnessMapChannel = FindProperty("_SmoothnessTextureChannel", props, false);
            highlights = FindProperty("_SpecularHighlights", props, false);
            reflections = FindProperty("_GlossyReflections", props, false);
            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);
            heigtMapScale = FindProperty("_Parallax", props);
            heightMap = FindProperty("_ParallaxMap", props);
            occlusionStrength = FindProperty("_OcclusionStrength", props);
            occlusionMap = FindProperty("_OcclusionMap", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            detailMask = FindProperty("_DetailMask", props);
            detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
            detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
            detailNormalMap = FindProperty("_DetailNormalMap", props);
            uvSetSecondary = FindProperty("_UVSec", props);

            proceduralSnow = FindProperty("_ProceduralSnow", props, true);
            proceduralLayer = FindProperty("_ProceduralLayer", props, true);
            snowMap = FindProperty("_SnowTexture", props);
            snowColor = FindProperty("_SnowColor", props);
            snowTiling = FindProperty("_SnowTiling", props);
            snowDirection = FindProperty("_SnowDirection", props);
            snowLevel = FindProperty("_SnowLevel", props);
            snowSmoothness = FindProperty("_SnowSmoothness", props);
            doubleSided = FindProperty("_DoubleSidedEnable", props);

            wind = FindProperty("_Wind", props, true);
            isLeaves = FindProperty("_IsLeaves", props, false);
            isBark = FindProperty("_IsBark", props, false);
            isGrass = FindProperty("_IsGrass", props, false);
            baseWindMultipliers = FindProperty("_BaseWindMultipliers", props, false);
            tumbleStrength = FindProperty("_TumbleStrength", props, false);
            tumbleFrequency = FindProperty("_TumbleFrequency", props, false);
            timeOffset = FindProperty("_TimeOffset", props, false);
            enableLeafTurbulence = FindProperty("_EnableLeafTurbulence", props, false);
            leafTurbulence = FindProperty("_LeafTurbulence", props, false);
            edgeFlutterInfluence = FindProperty("_EdgeFlutterInfluence", props, false);
            enableAdvancedEdgeBending = FindProperty("_EnableAdvancedEdgeBending", props, false);
            advancedEdgeBending = FindProperty("_AdvancedEdgeBending", props, false);

            windTime = FindProperty("_ShakeTime", props, false);
            windSpeed = FindProperty("_ShakeWindspeed", props, false);
            windBending = FindProperty("_ShakeBending", props, false);

            flatShading = FindProperty("_FlatShading", props, false);

            detailMapUV3 = FindProperty("_DetalUseUV3", props, false);

            colormapBlending = FindProperty("_ColormapBlending", props, false);
            colormap = FindProperty("_Colormap", props, false);
            colormapInfluence = FindProperty("_ColormapInfluence", props, false);
            worldSize = FindProperty("_WorldSize", props, false);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                CheckTessMode();
                MaterialChanged(material, m_WorkflowMode, m_TessWorkflowMode);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                BlendModePopup();

                GUILayout.Label(Styles.tessellationText, EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(tessmode, Styles.tessModeText.text);
                CheckTessMode();
                if (m_TessWorkflowMode == TessWorkflowMode.Distance)
                {
                    m_MaterialEditor.ShaderProperty(tessellation, Styles.tessFacText.text);
                    m_MaterialEditor.ShaderProperty(maxdist, Styles.tessMaxText.text);
                }
                else
                {
                    m_MaterialEditor.ShaderProperty(tessellation, Styles.edgelenText.text);
                }
                m_MaterialEditor.ShaderProperty(shadowgen, Styles.shoadowGenText.text);
                EditorGUILayout.Space();

                GUILayout.Label(Styles.displacementText, EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(phong, Styles.phongText.text);
                m_MaterialEditor.ShaderProperty(displacement, Styles.dispScaleText.text);
                m_MaterialEditor.ShaderProperty(dispoffset, Styles.dispOffsetText.text);
                EditorGUILayout.Space();

                // Primary properties
                GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
                DoAlbedoArea(material);
                DoSpecularMetallicArea();
                m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
                m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap);
                m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
                DoEmissionArea(material);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                if (EditorGUI.EndChangeCheck())
                    emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake

                EditorGUILayout.Space();

                // Secondary properties
                GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
                m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
                m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

                // Third properties
                GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
                if (highlights != null)
                    m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
                if (reflections != null)
                    m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);

                TerraUnity.Runtime.UI.THelpersUIRuntime.GUI_HelpBoxTitleWide(new GUIContent("TERRAWORLD FEATURES", "TERRAWORLD FEATURES"), 30, TerraUnity.Runtime.UI.THelpersUIRuntime.UIColor);
                GUILayout.Space(10);

                // Procedural Snow properties
                GUILayout.Label(Styles.snowText, EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(proceduralLayer, "Enable Layer");
                m_MaterialEditor.ShaderProperty(proceduralSnow, "Global Snow");

                if (proceduralLayer.floatValue == 1)
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.snowMapText, snowMap, snowColor);
                    m_MaterialEditor.ShaderProperty(snowTiling, "Tiling");
                    m_MaterialEditor.ShaderProperty(snowDirection, "Direction");
                    m_MaterialEditor.ShaderProperty(snowLevel, "Strength");
                    m_MaterialEditor.ShaderProperty(snowSmoothness, "Smoothness");
                }

                GUILayout.Space(20);

                // Double-Sided material rendering
                GUILayout.Label("Double Sided Rendeing", EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(doubleSided, "Enable Double Sided");

                if (material.GetFloat("_DoubleSidedEnable") == 1)
                    material.SetInt("_CullMode", 0);
                else
                    material.SetInt("_CullMode", 2);

                GUILayout.Space(20);

                // Wind properties
                GUILayout.Label("Wind Settings", EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(wind, "Enable Wind");

                if (wind.floatValue == 1)
                {
                    m_MaterialEditor.ShaderProperty(isLeaves, "Leaves Rendering");
                    m_MaterialEditor.ShaderProperty(isBark, "Bark Rendering");
                    m_MaterialEditor.ShaderProperty(isGrass, "Grass Rendering");

                    if (isLeaves.floatValue == 1)
                    {
                        m_MaterialEditor.ShaderProperty(baseWindMultipliers, "x: Main, y: Turbulence, Z: flutter");
                        m_MaterialEditor.ShaderProperty(tumbleStrength, "Tumble Strength");
                        m_MaterialEditor.ShaderProperty(tumbleFrequency, "Tumble Frequency");
                        m_MaterialEditor.ShaderProperty(timeOffset, "Time Offset");
                        m_MaterialEditor.ShaderProperty(enableLeafTurbulence, "Enable Leaf Turbulence");
                        m_MaterialEditor.ShaderProperty(leafTurbulence, "Leaf Turbulence");
                        m_MaterialEditor.ShaderProperty(edgeFlutterInfluence, "Edge Flutter Influence");
                        m_MaterialEditor.ShaderProperty(enableAdvancedEdgeBending, "Enable Advanced Edge Bending");
                        m_MaterialEditor.ShaderProperty(advancedEdgeBending, "Advanced Edge Bending");
                    }
                    else if (isBark.floatValue == 1)
                    {
                        m_MaterialEditor.ShaderProperty(baseWindMultipliers, "x: Main, y: Turbulence, Z: flutter");
                        m_MaterialEditor.ShaderProperty(tumbleStrength, "Tumble Strength");
                        m_MaterialEditor.ShaderProperty(tumbleFrequency, "Tumble Frequency");
                        m_MaterialEditor.ShaderProperty(timeOffset, "Time Offset");
                    }
                    else if (isGrass.floatValue == 1)
                    {
                        m_MaterialEditor.ShaderProperty(windTime, "Wind Time");
                        m_MaterialEditor.ShaderProperty(windSpeed, "Wind Speed");
                        m_MaterialEditor.ShaderProperty(windBending, "Wind Bending");
                    }
                    else
                    {
                        m_MaterialEditor.ShaderProperty(baseWindMultipliers, "x: Main, y: Turbulence, Z: flutter");
                        m_MaterialEditor.ShaderProperty(tumbleStrength, "Tumble Strength");
                        m_MaterialEditor.ShaderProperty(tumbleFrequency, "Tumble Frequency");
                        m_MaterialEditor.ShaderProperty(timeOffset, "Time Offset");
                        m_MaterialEditor.ShaderProperty(enableLeafTurbulence, "Enable Leaf Turbulence");
                        m_MaterialEditor.ShaderProperty(leafTurbulence, "Leaf Turbulence");
                        m_MaterialEditor.ShaderProperty(edgeFlutterInfluence, "Edge Flutter Influence");
                        m_MaterialEditor.ShaderProperty(enableAdvancedEdgeBending, "Enable Advanced Edge Bending");
                        m_MaterialEditor.ShaderProperty(advancedEdgeBending, "Advanced Edge Bending");
                    }

                    //m_MaterialEditor.ShaderProperty(windParams1, "Wind Params 1");
                    //m_MaterialEditor.ShaderProperty(windParams2, "Wind Params 2");
                }

                GUILayout.Space(20);

                // Flat Shading properties
                GUILayout.Label("Flat Shading Settings", EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(flatShading, "Enable Flat Shading");

                GUILayout.Space(20);

                // NatureManufacture Support properties
                GUILayout.Label("NatureManufacture Options", EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(detailMapUV3, "Enable UV3 DetailMap");

                GUILayout.Space(20);

                // Colormap Blending with global satellite image or top-down world snapshot
                GUILayout.Label("Colormap Blending", EditorStyles.boldLabel);
                m_MaterialEditor.ShaderProperty(colormapBlending, "Colormap Blending");

                if (colormapBlending.floatValue == 1)
                {
                    m_MaterialEditor.ShaderProperty(colormap, "Colormap Texture");
                    m_MaterialEditor.ShaderProperty(colormapInfluence, "Bottom To Top Influence");
                    m_MaterialEditor.ShaderProperty(worldSize, "World Size");
                }

                GUILayout.Space(10);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj, m_WorkflowMode, m_TessWorkflowMode);
            }

            EditorGUILayout.Space();

            GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
            m_MaterialEditor.RenderQueueField();
            m_MaterialEditor.EnableInstancingField();
        }

        internal void DetermineWorkflow(MaterialProperty[] props)
        {
            if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
                m_WorkflowMode = WorkflowMode.Specular;
            else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            else
                m_WorkflowMode = WorkflowMode.Dielectric;
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Mode", (float)blendMode);

            DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Material[] { material }));
            MaterialChanged(material, m_WorkflowMode, m_TessWorkflowMode);
        }

        void CheckTessMode()
        {
            if (tessmode != null)
            {
                int dispIs = (int)tessmode.floatValue;
                if (dispIs == (int)TessWorkflowMode.EdgeLength)
                {
                    m_TessWorkflowMode = TessWorkflowMode.EdgeLength;
                }
                else
                {
                    m_TessWorkflowMode = TessWorkflowMode.Distance;
                }
            }
        }


        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
            if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
            }
        }

        void DoEmissionArea(Material material)
        {
            // Emission for GI?
            if (m_MaterialEditor.EmissionEnabledProperty())
            {
                bool hadEmissionTexture = emissionMap.textureValue != null;

                // Texture and HDR color controls
                //m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, m_ColorPickerHDRConfig, false);
                m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);

                // If texture was assigned and color was black set color to white
                float brightness = emissionColorForRendering.colorValue.maxColorComponent;
                if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorForRendering.colorValue = Color.white;

                // change the GI flag and fix it up with emissive as black if necessary
                m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
            }
        }

        void DoSpecularMetallicArea()
        {
            bool hasGlossMap = false;
            if (m_WorkflowMode == WorkflowMode.Specular)
            {
                hasGlossMap = specularMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap, hasGlossMap ? null : specularColor);
            }
            else if (m_WorkflowMode == WorkflowMode.Metallic)
            {
                hasGlossMap = metallicMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, hasGlossMap ? null : metallic);
            }

            bool showSmoothnessScale = hasGlossMap;
            if (smoothnessMapChannel != null)
            {
                int smoothnessChannel = (int)smoothnessMapChannel.floatValue;
                if (smoothnessChannel == (int)SmoothnessMapChannel.AlbedoAlpha)
                    showSmoothnessScale = true;
            }

            int indentation = 2; // align with labels of texture properties
            m_MaterialEditor.ShaderProperty(showSmoothnessScale ? smoothnessScale : smoothness, showSmoothnessScale ? Styles.smoothnessScaleText : Styles.smoothnessText, indentation);

            ++indentation;
            if (smoothnessMapChannel != null)
                m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, indentation);
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
            if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;
            else
                return SmoothnessMapChannel.SpecularMetallicAlpha;
        }

        static void SetMaterialKeywords(Material material, WorkflowMode workflowMode, TessWorkflowMode tessWorkflowMode)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
            if (workflowMode == WorkflowMode.Specular)
                SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
            else if (workflowMode == WorkflowMode.Metallic)
                SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
            SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));
            SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap"));

            // We're setting this so that if the shader Falls back to no tessellation, the parallax height would be right
            material.SetFloat("_Parallax", Mathf.Lerp(0.005f, 0.08f, material.GetFloat("_Displacement")));

            if (tessWorkflowMode == TessWorkflowMode.EdgeLength)
            {
                SetKeyword(material, "FT_EDGE_TESS", true);
            }
            else
            {
                SetKeyword(material, "FT_EDGE_TESS", false);
            }

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
            }

            if (material.GetFloat("_ProceduralLayer") == 1)
                SetKeyword(material, "_PROCEDURALSNOW", true);
            else
                SetKeyword(material, "_PROCEDURALSNOW", false);

            if (material.GetFloat("_Wind") == 1)
                SetKeyword(material, "_WIND", true);
            else
                SetKeyword(material, "_WIND", false);

            if (material.GetFloat("_IsLeaves") == 1)
                SetKeyword(material, "_IS_LEAVES", true);
            else
                SetKeyword(material, "_IS_LEAVES", false);

            if (material.GetFloat("_IsBark") == 1)
                SetKeyword(material, "_IS_BARK", true);
            else
                SetKeyword(material, "_IS_BARK", false);

            if (material.GetFloat("_IsGrass") == 1)
                SetKeyword(material, "_IS_GRASS", true);
            else
                SetKeyword(material, "_IS_GRASS", false);

            if (material.GetFloat("_FlatShading") == 1)
                SetKeyword(material, "_FLATSHADING", true);
            else
                SetKeyword(material, "_FLATSHADING", false);

            if (material.GetFloat("_DetalUseUV3") == 1)
                SetKeyword(material, "_DETALUSEUV3_ON", true);
            else
                SetKeyword(material, "_DETALUSEUV3_ON", false);

            if (material.GetFloat("_ColormapBlending") == 1)
                SetKeyword(material, "_COLORMAP_BLENDING", true);
            else
                SetKeyword(material, "_COLORMAP_BLENDING", false);
        }

        static void MaterialChanged(Material material, WorkflowMode workflowMode, TessWorkflowMode tessWorkflowMode)
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

            SetMaterialKeywords(material, workflowMode, tessWorkflowMode);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
} // namespace UnityEditor

