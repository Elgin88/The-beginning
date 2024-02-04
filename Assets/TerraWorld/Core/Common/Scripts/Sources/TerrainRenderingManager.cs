#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;
using TerraUnity.Runtime;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    enum TerrainShaderType
    {
        TerraFormer,
        TerraFormerInstanced,
        Other
    }

    public struct TerrainRenderingParams
    {
        public string worldName;
        public bool modernRendering;
        public bool tessellation;
        public bool heightmapBlending;
        public bool colormapBlending;
        //public bool proceduralSnow;
        public bool proceduralPuddles;
        public bool isFlatShading;

        // Surface Tint
        //-----------------------------------------------------------------------
        public System.Numerics.Vector4 surfaceTintColorMAIN;
        public System.Numerics.Vector4 surfaceTintColorBG;

        // Tessellation
        //-----------------------------------------------------------------------
        [Range(0f, 100f)] public float tessellationQuality;
        [Range(0f, 10f)] public float edgeSmoothness;
        [Range(0.1f, 32f)] public float displacement1;
        [Range(0.1f, 32f)] public float displacement2;
        [Range(0.1f, 32f)] public float displacement3;
        [Range(0.1f, 32f)] public float displacement4;
        [Range(0.1f, 32f)] public float displacement5;
        [Range(0.1f, 32f)] public float displacement6;
        [Range(0.1f, 32f)] public float displacement7;
        [Range(0.1f, 32f)] public float displacement8;
        [Range(0.1f, 8f)] public float heightOffset1;
        [Range(0.1f, 8f)] public float heightOffset2;
        [Range(0.1f, 8f)] public float heightOffset3;
        [Range(0.1f, 8f)] public float heightOffset4;
        [Range(0.1f, 8f)] public float heightOffset5;
        [Range(0.1f, 8f)] public float heightOffset6;
        [Range(0.1f, 8f)] public float heightOffset7;
        [Range(0.1f, 8f)] public float heightOffset8;

        // Heightmap Blending
        //-----------------------------------------------------------------------
        public float heightBlending;

        // Tiling Remover
        //-----------------------------------------------------------------------
        [Range(0f, 64f)] public float tilingRemover1;
        [Range(0f, 64f)] public float tilingRemover2;
        [Range(0f, 64f)] public float tilingRemover3;
        [Range(0f, 64f)] public float tilingRemover4;
        [Range(0f, 64f)] public float tilingRemover5;
        [Range(0f, 64f)] public float tilingRemover6;
        [Range(0f, 64f)] public float tilingRemover7;
        [Range(0f, 64f)] public float tilingRemover8;
        [Range(0f, 200f)] public float noiseTiling1;
        [Range(0f, 200f)] public float noiseTiling2;
        [Range(0f, 200f)] public float noiseTiling3;
        [Range(0f, 200f)] public float noiseTiling4;
        [Range(0f, 200f)] public float noiseTiling5;
        [Range(0f, 200f)] public float noiseTiling6;
        [Range(0f, 200f)] public float noiseTiling7;
        [Range(0f, 200f)] public float noiseTiling8;

        // Colormap Blending
        //-----------------------------------------------------------------------
        public float colormapBlendingDistance;

        // Procedural Snow
        //-----------------------------------------------------------------------
        public float snowColorR;
        public float snowColorG;
        public float snowColorB;
        public float snowTiling;
        public float snowAmount;
        public float snowAngles;
        public float snowNormalInfluence;
        public float snowPower;
        //public float snowMetallic;
        public float snowSmoothness;
        //public float snowNormalPower;
        public float snowStartHeight;
        public float heightFalloff;
        public float snowThickness;
        public float snowdamping;

        // Procedural Puddles
        //-----------------------------------------------------------------------
        public float puddleColorR;
        public float puddleColorG;
        public float puddleColorB;
        public float puddleRefraction;
        public float puddleMetallic;
        public float puddleSmoothness;
        public float puddlewaterHeight;
        public float puddleSlope;
        public float puddleMinSlope;
        public float puddleNoiseTiling;
        public float puddleNoiseInfluence;
        public bool puddleReflections;

        // Layer Properties
        //-----------------------------------------------------------------------
        public float layerColor1R;
        public float layerColor1G;
        public float layerColor1B;
        public float layerColor2R;
        public float layerColor2G;
        public float layerColor2B;
        public float layerColor3R;
        public float layerColor3G;
        public float layerColor3B;
        public float layerColor4R;
        public float layerColor4G;
        public float layerColor4B;
        public float layerColor5R;
        public float layerColor5G;
        public float layerColor5B;
        public float layerColor6R;
        public float layerColor6G;
        public float layerColor6B;
        public float layerColor7R;
        public float layerColor7G;
        public float layerColor7B;
        public float layerColor8R;
        public float layerColor8G;
        public float layerColor8B;
        public float layerAO1;
        public float layerAO2;
        public float layerAO3;
        public float layerAO4;
        public float layerAO5;
        public float layerAO6;
        public float layerAO7;
        public float layerAO8;

        //public float layerNormalScale1;
        //public float layerMetallic1;
        //public float layerSmoothness1;
        //[XmlIgnore] public Vector4 layerTiling1;
        //public float layerNormalScale2;
        //public float layerMetallic2;
        //public float layerSmoothness2;
        //[XmlIgnore] public Vector4 layerTiling2;
        //public float layerNormalScale3;
        //public float layerMetallic3;
        //public float layerSmoothness3;
        //[XmlIgnore] public Vector4 layerTiling3;
        //public float layerNormalScale4;
        //public float layerMetallic4;
        //public float layerSmoothness4;
        //[XmlIgnore] public Vector4 layerTiling4;
        //public float layerNormalScale5;
        //public float layerMetallic5;
        //public float layerSmoothness5;
        //[XmlIgnore] public Vector4 layerTiling5;
        //public float layerNormalScale6;
        //public float layerMetallic6;
        //public float layerSmoothness6;
        //[XmlIgnore] public Vector4 layerTiling6;
        //public float layerNormalScale7;
        //public float layerMetallic7;
        //public float layerSmoothness7;
        //[XmlIgnore] public Vector4 layerTiling7;
        //public float layerNormalScale8;
        //public float layerMetallic8;
        //public float layerSmoothness8;
        //[XmlIgnore] public Vector4 layerTiling8;

        // Global Texturing
        public float proceduralNoiseScale;
        public bool proceduralTexturing1;
        public bool gradientTexturing1;
        public float gradientTiling1;
        public float colorDamping1;
        public float slopeInfluence1;
        public bool proceduralTexturing2;
        public bool gradientTexturing2;
        public float gradientTiling2;
        public float colorDamping2;
        public float slopeInfluence2;
        public bool proceduralTexturing3;
        public bool gradientTexturing3;
        public float gradientTiling3;
        public float colorDamping3;
        public float slopeInfluence3;
        public bool proceduralTexturing4;
        public bool gradientTexturing4;
        public float gradientTiling4;
        public float colorDamping4;
        public float slopeInfluence4;
        public bool proceduralTexturing5;
        public bool gradientTexturing5;
        public float gradientTiling5;
        public float colorDamping5;
        public float slopeInfluence5;
        public bool proceduralTexturing6;
        public bool gradientTexturing6;
        public float gradientTiling6;
        public float colorDamping6;
        public float slopeInfluence6;
        public bool proceduralTexturing7;
        public bool gradientTexturing7;
        public float gradientTiling7;
        public float colorDamping7;
        public float slopeInfluence7;
        public bool proceduralTexturing8;
        public bool gradientTexturing8;
        public float gradientTiling8;
        public float colorDamping8;
        public float slopeInfluence8;

#if !TERRAWORLD_XPRO
        // Splatmap Settings
        //-----------------------------------------------------------------------
        public bool splatmapResolutionBestFit;
        public int splatmapSmoothness;
        public int splatmapResolution;

        // Main Terrain Settings
        //-----------------------------------------------------------------------
        public int terrainPixelError;

        // Background Terrain Settings
        //-----------------------------------------------------------------------
        public bool BGMountains;
        public int BGTerrainScaleMultiplier;
        public int BGTerrainHeightmapResolution;
        public int BGTerrainSatelliteImageResolution;
        public int BGTerrainPixelError;
        public float BGTerrainOffset;
#endif

        public TerrainRenderingParams(bool newparameters = false)
        {
            if (!newparameters) throw new Exception("Undefined TerrainRenderingParams ");
            worldName = "";

            // Global Settings
            //-----------------------------------------------------------------------
            modernRendering = true;
            tessellation = false;
            heightmapBlending = false;
            colormapBlending = true;
            //proceduralSnow = true;
            proceduralPuddles = false;
            isFlatShading = false;

            // Surface Tint
            //-----------------------------------------------------------------------
            surfaceTintColorMAIN = new System.Numerics.Vector4(1f, 1f, 1f, 1f);
            surfaceTintColorBG = new System.Numerics.Vector4(1f, 1f, 1f, 1f);

            // Tessellation
            //-----------------------------------------------------------------------
            tessellationQuality = 25f;
            edgeSmoothness = 1f;
            displacement1 = 0.1f;
            displacement2 = 0.1f;
            displacement3 = 0.1f;
            displacement4 = 0.1f;
            displacement5 = 0.1f;
            displacement6 = 0.1f;
            displacement7 = 0.1f;
            displacement8 = 0.1f;
            heightOffset1 = 0.1f;
            heightOffset2 = 0.1f;
            heightOffset3 = 0.1f;
            heightOffset4 = 0.1f;
            heightOffset5 = 0.1f;
            heightOffset6 = 0.1f;
            heightOffset7 = 0.1f;
            heightOffset8 = 0.1f;

            // Heightmap Blending
            //-----------------------------------------------------------------------
            heightBlending = 0.25f;

            // Tiling Remover
            //-----------------------------------------------------------------------
            tilingRemover1 = 0f;
            tilingRemover2 = 0f;
            tilingRemover3 = 0f;
            tilingRemover4 = 0f;
            tilingRemover5 = 0f;
            tilingRemover6 = 0f;
            tilingRemover7 = 0f;
            tilingRemover8 = 0f;
            noiseTiling1 = 64f;
            noiseTiling2 = 64f;
            noiseTiling3 = 64f;
            noiseTiling4 = 64f;
            noiseTiling5 = 64f;
            noiseTiling6 = 64f;
            noiseTiling7 = 64f;
            noiseTiling8 = 64f;

            // Colormap Blending
            //-----------------------------------------------------------------------
            colormapBlendingDistance = 8000f;

            // Procedural Snow
            //-----------------------------------------------------------------------
            snowColorR = 0.25f;
            snowColorG = 0.25f;
            snowColorB = 0.25f;
            snowTiling = 2000f;
            snowAmount = 0.75f;
            snowAngles = -0.7f;
            snowNormalInfluence = 0.5f;
            snowPower = 0.8f;
            snowSmoothness = 1f;
            snowStartHeight = 3500f;
            heightFalloff = 1000f;
            snowThickness = 0f;
            snowdamping = 0f;

            // Procedural Puddles
            //-----------------------------------------------------------------------
            puddleColorR = 0.85f;
            puddleColorG = 0.85f;
            puddleColorB = 0.85f;
            puddleRefraction = 0.07f;
            puddleMetallic = 0.925f;
            puddleSmoothness = 0.95f;
            puddlewaterHeight = 1f;
            puddleSlope = 0.004f;
            puddleMinSlope = 0.0025f;
            puddleNoiseTiling = 300f;
            puddleNoiseInfluence = 0.1f;
            puddleReflections = false;

            // Layer Properties
            //-----------------------------------------------------------------------
            layerColor1R = 0.5f;
            layerColor1G = 0.5f;
            layerColor1B = 0.5f;
            layerColor2R = 0.5f;
            layerColor2G = 0.5f;
            layerColor2B = 0.5f;
            layerColor3R = 0.5f;
            layerColor3G = 0.5f;
            layerColor3B = 0.5f;
            layerColor4R = 0.5f;
            layerColor4G = 0.5f;
            layerColor4B = 0.5f;
            layerColor5R = 0.5f;
            layerColor5G = 0.5f;
            layerColor5B = 0.5f;
            layerColor6R = 0.5f;
            layerColor6G = 0.5f;
            layerColor6B = 0.5f;
            layerColor7R = 0.5f;
            layerColor7G = 0.5f;
            layerColor7B = 0.5f;
            layerColor8R = 0.5f;
            layerColor8G = 0.5f;
            layerColor8B = 0.5f;
            layerAO1 = 1f;
            layerAO2 = 1f;
            layerAO3 = 1f;
            layerAO4 = 1f;
            layerAO5 = 1f;
            layerAO6 = 1f;
            layerAO7 = 1f;
            layerAO8 = 1f;
            //layerNormalScale1 = 1;
            //layerMetallic1 = 0.1f;
            //layerSmoothness1 = 0.1f;
            //layerTiling1 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY
            //layerNormalScale2 = 1;
            //layerMetallic2 = 0.1f;
            //layerSmoothness2 = 0.1f;
            //layerTiling2 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY
            //layerNormalScale3 = 1;
            //layerMetallic3 = 0.1f;
            //layerSmoothness3 = 0.1f;
            //layerTiling3 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY
            //layerNormalScale4 = 1;
            //layerMetallic4 = 0.1f;
            //layerSmoothness4 = 0.1f;
            //layerTiling4 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY
            //layerNormalScale5 = 1;
            //layerMetallic5 = 0.1f;
            //layerSmoothness5 = 0.1f;
            //layerTiling5 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY
            //layerNormalScale6 = 1;
            //layerMetallic6 = 0.1f;
            //layerSmoothness6 = 0.1f;
            //layerTiling6 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY
            //layerNormalScale7 = 1;
            //layerMetallic7 = 0.1f;
            //layerSmoothness7 = 0.1f;
            //layerTiling7 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY
            //layerNormalScale8 = 1;
            //layerMetallic8 = 0.1f;
            //layerSmoothness8 = 0.1f;
            //layerTiling8 = new Vector4(4, 4, 0, 0); // x = SizeX, y = SizeY, z = OffsetX, w = OffsetY

            // Global Texturing
            proceduralNoiseScale = 256f;
            proceduralTexturing1 = false;
            gradientTexturing1 = false;
            gradientTiling1 = 1;
            colorDamping1 = 0.05f;
            slopeInfluence1 = 1;
            proceduralTexturing2 = false;
            gradientTexturing2 = false;
            gradientTiling2 = 1;
            colorDamping2 = 0.05f;
            slopeInfluence2 = 1;
            proceduralTexturing3 = false;
            gradientTexturing3 = false;
            gradientTiling3 = 1;
            colorDamping3 = 0.05f;
            slopeInfluence3 = 1;
            proceduralTexturing4 = false;
            gradientTexturing4 = false;
            gradientTiling4 = 1;
            colorDamping4 = 0.05f;
            slopeInfluence4 = 1;
            proceduralTexturing5 = false;
            gradientTexturing5 = false;
            gradientTiling5 = 1;
            colorDamping5 = 0.05f;
            slopeInfluence5 = 1;
            proceduralTexturing6 = false;
            gradientTexturing6 = false;
            gradientTiling6 = 1;
            colorDamping6 = 0.05f;
            slopeInfluence6 = 1;
            proceduralTexturing7 = false;
            gradientTexturing7 = false;
            gradientTiling7 = 1;
            colorDamping7 = 0.05f;
            slopeInfluence7 = 1;
            proceduralTexturing8 = false;
            gradientTexturing8 = false;
            gradientTiling8 = 1;
            colorDamping8 = 0.05f;
            slopeInfluence8 = 1;

#if !TERRAWORLD_XPRO
            splatmapResolutionBestFit = true;
            splatmapSmoothness = 1;
            splatmapResolution = 512;
            terrainPixelError = 5;
            BGMountains = true;
            BGTerrainScaleMultiplier = 8;
            BGTerrainHeightmapResolution = 512;
            BGTerrainSatelliteImageResolution = 2048;
            BGTerrainPixelError = 40;
            BGTerrainOffset = 0f;
#endif
        }

        public void FilterCopy (TerrainRenderingParams newRenderingParams,
                bool Ignore_RenderingGraph_surfaceTint,
                bool Ignore_RenderingGraph_modernRendering,
                bool Ignore_RenderingGraph_tessellation,
                bool Ignore_RenderingGraph_heightmapBlending,
                bool Ignore_RenderingGraph_TillingRemover,
                bool Ignore_RenderingGraph_colormapBlending,
                bool Ignore_RenderingGraph_proceduralSnow,
                bool Ignore_RenderingGraph_proceduralPuddles,
                bool Ignore_RenderingGraph_LayerProperties,
                bool Ignore_RenderingGraph_isFlatShading,
                bool Ignore_RenderingGraph_SplatmapSettings,
                bool Ignore_RenderingGraph_BGTerrainSettings
            )
        {
            worldName = newRenderingParams.worldName;

            if (!Ignore_RenderingGraph_surfaceTint)
            {
                surfaceTintColorMAIN = newRenderingParams.surfaceTintColorMAIN;
                surfaceTintColorBG = newRenderingParams.surfaceTintColorBG;
            }

            if (!Ignore_RenderingGraph_modernRendering)
                modernRendering = newRenderingParams.modernRendering;

            //if (Ignore_RenderingGraph_instancedDrawing)
            //{
            //    instancedDrawing = newRenderingParams.instancedDrawing;
            //}

            // Tessellation
            //-----------------------------------------------------------------------
            if (!Ignore_RenderingGraph_tessellation)
            {
                tessellation = newRenderingParams.tessellation;
                tessellationQuality = newRenderingParams.tessellationQuality;
                edgeSmoothness = newRenderingParams.edgeSmoothness;
                displacement1 = newRenderingParams.displacement1;
                displacement2 = newRenderingParams.displacement2;
                displacement3 = newRenderingParams.displacement3;
                displacement4 = newRenderingParams.displacement4;
                displacement5 = newRenderingParams.displacement5;
                displacement6 = newRenderingParams.displacement6;
                displacement7 = newRenderingParams.displacement7;
                displacement8 = newRenderingParams.displacement8;
                heightOffset1 = newRenderingParams.heightOffset1;
                heightOffset2 = newRenderingParams.heightOffset2;
                heightOffset3 = newRenderingParams.heightOffset3;
                heightOffset4 = newRenderingParams.heightOffset4;
                heightOffset5 = newRenderingParams.heightOffset5;
                heightOffset6 = newRenderingParams.heightOffset6;
                heightOffset7 = newRenderingParams.heightOffset7;
                heightOffset8 = newRenderingParams.heightOffset8;
            }

            // Heightmap Blending
            //-----------------------------------------------------------------------
            if (!Ignore_RenderingGraph_heightmapBlending)
            {
                heightmapBlending = newRenderingParams.heightmapBlending;
                heightBlending = newRenderingParams.heightBlending;
            }

            // Tiling Remover
            //-----------------------------------------------------------------------
            if (!Ignore_RenderingGraph_TillingRemover)
            {
                tilingRemover1 = newRenderingParams.tilingRemover1;
                tilingRemover2 = newRenderingParams.tilingRemover2;
                tilingRemover3 = newRenderingParams.tilingRemover3;
                tilingRemover4 = newRenderingParams.tilingRemover4;
                tilingRemover5 = newRenderingParams.tilingRemover5;
                tilingRemover6 = newRenderingParams.tilingRemover6;
                tilingRemover7 = newRenderingParams.tilingRemover7;
                tilingRemover8 = newRenderingParams.tilingRemover8;

                noiseTiling1 = newRenderingParams.noiseTiling1;
                noiseTiling2 = newRenderingParams.noiseTiling2;
                noiseTiling3 = newRenderingParams.noiseTiling3;
                noiseTiling4 = newRenderingParams.noiseTiling4;
                noiseTiling5 = newRenderingParams.noiseTiling5;
                noiseTiling6 = newRenderingParams.noiseTiling6;
                noiseTiling7 = newRenderingParams.noiseTiling7;
                noiseTiling8 = newRenderingParams.noiseTiling8;
            }

            // Colormap Blending
            //-----------------------------------------------------------------------
            if (!Ignore_RenderingGraph_colormapBlending)
            {
                colormapBlending = newRenderingParams.colormapBlending;
                colormapBlendingDistance = newRenderingParams.colormapBlendingDistance;
            }

            // Procedural Snow
            //-----------------------------------------------------------------------
            if (!Ignore_RenderingGraph_proceduralSnow)
            {
                //proceduralSnow = newRenderingParams.proceduralSnow;
                snowColorR = newRenderingParams.snowColorR;
                snowColorG = newRenderingParams.snowColorG;
                snowColorB = newRenderingParams.snowColorB;
                snowTiling = newRenderingParams.snowTiling;
                snowAmount = newRenderingParams.snowAmount;
                snowAngles = newRenderingParams.snowAngles;
                snowNormalInfluence = newRenderingParams.snowNormalInfluence;
                snowPower = newRenderingParams.snowPower;
                snowSmoothness = newRenderingParams.snowSmoothness;
                snowStartHeight = newRenderingParams.snowStartHeight;
                heightFalloff = newRenderingParams.heightFalloff;
            }

            // Procedural Puddles
            //-----------------------------------------------------------------------
            if (!Ignore_RenderingGraph_proceduralPuddles)
            {
                proceduralPuddles = newRenderingParams.proceduralPuddles;
                puddleColorR = newRenderingParams.puddleColorR;
                puddleColorG = newRenderingParams.puddleColorG;
                puddleColorB = newRenderingParams.puddleColorB;
                puddleRefraction = newRenderingParams.puddleRefraction;
                puddleMetallic = newRenderingParams.puddleMetallic;
                puddleSmoothness = newRenderingParams.puddleSmoothness;
                puddlewaterHeight = newRenderingParams.puddlewaterHeight;
                puddleSlope = newRenderingParams.puddleSlope;
                puddleMinSlope = newRenderingParams.puddleMinSlope;
                puddleNoiseTiling = newRenderingParams.puddleNoiseTiling;
                puddleNoiseInfluence = newRenderingParams.puddleNoiseInfluence;
                puddleReflections = newRenderingParams.puddleReflections;
            }

            if (!Ignore_RenderingGraph_LayerProperties)
            {
                layerColor1R = newRenderingParams.layerColor1R;
                layerColor1G = newRenderingParams.layerColor1G;
                layerColor1B = newRenderingParams.layerColor1B;
                layerColor2R = newRenderingParams.layerColor2R;
                layerColor2G = newRenderingParams.layerColor2G;
                layerColor2B = newRenderingParams.layerColor2B;
                layerColor3R = newRenderingParams.layerColor3R;
                layerColor3G = newRenderingParams.layerColor3G;
                layerColor3B = newRenderingParams.layerColor3B;
                layerColor4R = newRenderingParams.layerColor4R;
                layerColor4G = newRenderingParams.layerColor4G;
                layerColor4B = newRenderingParams.layerColor4B;
                layerColor5R = newRenderingParams.layerColor5R;
                layerColor5G = newRenderingParams.layerColor5G;
                layerColor5B = newRenderingParams.layerColor5B;
                layerColor6R = newRenderingParams.layerColor6R;
                layerColor6G = newRenderingParams.layerColor6G;
                layerColor6B = newRenderingParams.layerColor6B;
                layerColor7R = newRenderingParams.layerColor7R;
                layerColor7G = newRenderingParams.layerColor7G;
                layerColor7B = newRenderingParams.layerColor7B;
                layerColor8R = newRenderingParams.layerColor8R;
                layerColor8G = newRenderingParams.layerColor8G;
                layerColor8B = newRenderingParams.layerColor8B;
                layerAO1 = newRenderingParams.layerAO1;
                layerAO2 = newRenderingParams.layerAO2;
                layerAO3 = newRenderingParams.layerAO3;
                layerAO4 = newRenderingParams.layerAO4;
                layerAO5 = newRenderingParams.layerAO5;
                layerAO6 = newRenderingParams.layerAO6;
                layerAO7 = newRenderingParams.layerAO7;
                layerAO8 = newRenderingParams.layerAO8;
                //layerNormalScale1 = newRenderingParams.layerNormalScale1;
                //layerMetallic1 = newRenderingParams.layerMetallic1;
                //layerSmoothness1 = newRenderingParams.layerSmoothness1;
                //layerTiling1 = newRenderingParams.layerTiling1;
                //layerNormalScale2 = newRenderingParams.layerNormalScale2;
                //layerMetallic2 = newRenderingParams.layerMetallic2;
                //layerSmoothness2 = newRenderingParams.layerSmoothness2;
                //layerTiling2 = newRenderingParams.layerTiling2;
                //layerNormalScale3 = newRenderingParams.layerNormalScale3;
                //layerMetallic3 = newRenderingParams.layerMetallic3;
                //layerSmoothness3 = newRenderingParams.layerSmoothness3;
                //layerTiling3 = newRenderingParams.layerTiling3;
                //layerNormalScale4 = newRenderingParams.layerNormalScale4;
                //layerMetallic4 = newRenderingParams.layerMetallic4;
                //layerSmoothness4 = newRenderingParams.layerSmoothness4;
                //layerTiling4 = newRenderingParams.layerTiling4;
                //layerNormalScale5 = newRenderingParams.layerNormalScale5;
                //layerMetallic5 = newRenderingParams.layerMetallic5;
                //layerSmoothness5 = newRenderingParams.layerSmoothness5;
                //layerTiling5 = newRenderingParams.layerTiling5;
                //layerNormalScale6 = newRenderingParams.layerNormalScale6;
                //layerMetallic6 = newRenderingParams.layerMetallic6;
                //layerSmoothness6 = newRenderingParams.layerSmoothness6;
                //layerTiling6 = newRenderingParams.layerTiling6;
                //layerNormalScale7 = newRenderingParams.layerNormalScale7;
                //layerMetallic7 = newRenderingParams.layerMetallic7;
                //layerSmoothness7 = newRenderingParams.layerSmoothness7;
                //layerTiling7 = newRenderingParams.layerTiling7;
                //layerNormalScale8 = newRenderingParams.layerNormalScale8;
                //layerMetallic8 = newRenderingParams.layerMetallic8;
                //layerSmoothness8 = newRenderingParams.layerSmoothness8;
                //layerTiling8 = newRenderingParams.layerTiling8;

                // Global Texturing
                proceduralNoiseScale = newRenderingParams.proceduralNoiseScale;
                proceduralTexturing1 = newRenderingParams.proceduralTexturing1;
                gradientTexturing1 = newRenderingParams.gradientTexturing1;
                gradientTiling1 = newRenderingParams.gradientTiling1;
                colorDamping1 = newRenderingParams.colorDamping1;
                slopeInfluence1 = newRenderingParams.slopeInfluence1;
                proceduralTexturing2 = newRenderingParams.proceduralTexturing2;
                gradientTexturing2 = newRenderingParams.gradientTexturing2;
                gradientTiling2 = newRenderingParams.gradientTiling2;
                colorDamping2 = newRenderingParams.colorDamping2;
                slopeInfluence2 = newRenderingParams.slopeInfluence2;
                proceduralTexturing3 = newRenderingParams.proceduralTexturing3;
                gradientTexturing3 = newRenderingParams.gradientTexturing3;
                gradientTiling3 = newRenderingParams.gradientTiling3;
                colorDamping3 = newRenderingParams.colorDamping3;
                slopeInfluence3 = newRenderingParams.slopeInfluence3;
                proceduralTexturing4 = newRenderingParams.proceduralTexturing4;
                gradientTexturing4 = newRenderingParams.gradientTexturing4;
                gradientTiling4 = newRenderingParams.gradientTiling4;
                colorDamping4 = newRenderingParams.colorDamping4;
                slopeInfluence4 = newRenderingParams.slopeInfluence4;
                proceduralTexturing5 = newRenderingParams.proceduralTexturing5;
                gradientTexturing5 = newRenderingParams.gradientTexturing5;
                gradientTiling5 = newRenderingParams.gradientTiling5;
                colorDamping5 = newRenderingParams.colorDamping5;
                slopeInfluence5 = newRenderingParams.slopeInfluence5;
                proceduralTexturing6 = newRenderingParams.proceduralTexturing6;
                gradientTexturing6 = newRenderingParams.gradientTexturing6;
                gradientTiling6 = newRenderingParams.gradientTiling6;
                colorDamping6 = newRenderingParams.colorDamping6;
                slopeInfluence6 = newRenderingParams.slopeInfluence6;
                proceduralTexturing7 = newRenderingParams.proceduralTexturing7;
                gradientTexturing7 = newRenderingParams.gradientTexturing7;
                gradientTiling7 = newRenderingParams.gradientTiling7;
                colorDamping7 = newRenderingParams.colorDamping7;
                slopeInfluence7 = newRenderingParams.slopeInfluence7;
                proceduralTexturing8 = newRenderingParams.proceduralTexturing8;
                gradientTexturing8 = newRenderingParams.gradientTexturing8;
                gradientTiling8 = newRenderingParams.gradientTiling8;
                colorDamping8 = newRenderingParams.colorDamping8;
                slopeInfluence8 = newRenderingParams.slopeInfluence8;
            }

            if (!Ignore_RenderingGraph_isFlatShading)
                isFlatShading = newRenderingParams.isFlatShading;

            if (!Ignore_RenderingGraph_SplatmapSettings)
            {
#if !TERRAWORLD_XPRO
                splatmapResolutionBestFit = newRenderingParams.splatmapResolutionBestFit;
                splatmapSmoothness = newRenderingParams.splatmapSmoothness;
                splatmapResolution = newRenderingParams.splatmapResolution;
#endif
            }

            if (!Ignore_RenderingGraph_BGTerrainSettings)
            {
#if !TERRAWORLD_XPRO
                BGMountains = newRenderingParams.BGMountains;
                BGTerrainScaleMultiplier = newRenderingParams.BGTerrainScaleMultiplier;
                BGTerrainHeightmapResolution = newRenderingParams.BGTerrainHeightmapResolution;
                BGTerrainSatelliteImageResolution = newRenderingParams.BGTerrainSatelliteImageResolution;
                BGTerrainPixelError = newRenderingParams.BGTerrainPixelError;
                BGTerrainOffset = newRenderingParams.BGTerrainOffset;
#endif
            }

            this = newRenderingParams;
        }
    }

    public class TerrainRenderingManager
    {
        public static bool isProceduralSnow { get => IsTerrainProceduralSnow(); set { SetTerrainProceduralSnow(value); } }
        public static bool isProceduralSnowBG { get => IsTerrainProceduralSnowBG(); set { SetTerrainProceduralSnowBG(value); } }

        private static Texture2D colormapTexture;
        public static Texture2D ColormapTexture { get => GetColormapTexture(); set => SetColormapTexture(value); }
        private static Texture2D waterMaskTexture;
        private static Texture2D snowTexture;
        private static int terrainLayersCount;
        private static bool isModernRendering { get => IsModernRendering(); set { SetModernRendering(value); } }
        private static bool isTessellated { get => IsTerrainTessellation(); set { SetTerrainTessellation(value); } }
        private static bool isHeightmapBlending { get => IsTerrainHeightmapBlending(); set { SetTerrainHeightmapBlending(value); } }
        private static bool isColormapBlending { get => IsTerrainColormapBlending(); set { SetTerrainColormapBlending(value); } }
        private static bool isFlatShading { get => IsTerrainFlatShading(); set { SetTerrainFlatShading(value); } }
        private static bool isProceduralPuddles { get => IsTerrainProceduralPuddles(); set { SetTerrainProceduralPuddles(value); } }
        private static Color LightingColor { get => GetColor("_LightingColor"); set { SetColor("_LightingColor", value); } }
        private static Color LightingColorBG { get => GetColorBG("_LightingColor"); set { SetColorBG("_LightingColor", value); } }
        private static Color SnowColor { get => GetColor("_SnowColor"); set { SetColor("_SnowColor", value); } }
        private static float SnowStartHeight { get => GetFloat("_SnowStartHeight"); set { SetFloat("_SnowStartHeight", value); SetFloatBG("_SnowStartHeight", value); } }
        private static float HeightFalloff { get => GetFloat("_HeightFalloff"); set { SetFloat("_HeightFalloff", value); SetFloatBG("_HeightFalloff", value); } }
        private static float SnowTile { get => GetFloat("_SnowTile"); set { SetFloat("_SnowTile", value); } }
        private static float SnowAmount { get => GetFloat("_SnowAmount"); set { SetFloat("_SnowAmount", value); } }
        private static float SnowAngle { get => GetFloat("_SnowAngle"); set { SetFloat("_SnowAngle", value); } }
        private static float SnowNormalInfluence { get => GetFloat("_NormalInfluence"); set { SetFloat("_NormalInfluence", value); } }
        private static float SnowPower { get => GetFloat("_SnowPower"); set { SetFloat("_SnowPower", value); } }
        private static float SnowSmoothness { get => GetFloat("_SnowSmoothness"); set { SetFloat("_SnowSmoothness", value); } }
        private static Color PuddleColor { get => GetColor("_PuddleColor"); set { SetColor("_PuddleColor", value); } }
        private static float PuddleRefraction { get => GetFloat("_Refraction"); set { SetFloat("_Refraction", value); } }
        private static float PuddleMetallic { get => GetFloat("_PuddleMetallic"); set { SetFloat("_PuddleMetallic", value); } }
        private static float PuddleSmoothness { get => GetFloat("_PuddleSmoothness"); set { SetFloat("_PuddleSmoothness", value); } }
        private static float PuddleSlope { get => GetFloat("_Slope"); set { SetFloat("_Slope", value); } }
        private static float PuddleSlopeMin { get => GetFloat("_SlopeMin"); set { SetFloat("_SlopeMin", value); } }
        private static float PuddleNoiseTiling { get => GetFloat("_NoiseTiling"); set { SetFloat("_NoiseTiling", value); } }
        private static float PuddlewaterHeight { get => GetFloat("_WaterHeight"); set { SetFloat("_WaterHeight", value); } }
        private static float HeightmapBlending { get => GetFloat("_HeightmapBlending"); set { SetFloat("_HeightmapBlending", value); } }
        private static float ColormapBlendingDistance { get => GetFloat("_BlendingDistance"); set { SetFloat("_BlendingDistance", value); } }

        // Global Texturing
        private static float ProceduralNoiseScale { get => GetFloat("proceduralNoiseScale"); set { SetFloat("proceduralNoiseScale", value); } }
        private static float ProceduralTexturing1 { get => GetFloat("proceduralTexturing1"); set { SetFloat("proceduralTexturing1", value); } }
        private static float GradientTexturing1 { get => GetFloat("gradientTexturing1"); set { SetFloat("gradientTexturing1", value); } }
        private static float GradientTiling1 { get => GetFloat("gradientTiling1"); set { SetFloat("gradientTiling1", value); } }
        private static float ColorDamping1 { get => GetFloat("colorDamping1"); set { SetFloat("colorDamping1", value); } }
        private static float SlopeInfluence1 { get => GetFloat("slopeInfluence1"); set { SetFloat("slopeInfluence1", value); } }
        private static float ProceduralTexturing2 { get => GetFloat("proceduralTexturing2"); set { SetFloat("proceduralTexturing2", value); } }
        private static float GradientTexturing2 { get => GetFloat("gradientTexturing2"); set { SetFloat("gradientTexturing2", value); } }
        private static float GradientTiling2 { get => GetFloat("gradientTiling2"); set { SetFloat("gradientTiling2", value); } }
        private static float ColorDamping2 { get => GetFloat("colorDamping2"); set { SetFloat("colorDamping2", value); } }
        private static float SlopeInfluence2 { get => GetFloat("slopeInfluence2"); set { SetFloat("slopeInfluence2", value); } }
        private static float ProceduralTexturing3 { get => GetFloat("proceduralTexturing3"); set { SetFloat("proceduralTexturing3", value); } }
        private static float GradientTexturing3 { get => GetFloat("gradientTexturing3"); set { SetFloat("gradientTexturing3", value); } }
        private static float GradientTiling3 { get => GetFloat("gradientTiling3"); set { SetFloat("gradientTiling3", value); } }
        private static float ColorDamping3 { get => GetFloat("colorDamping3"); set { SetFloat("colorDamping3", value); } }
        private static float SlopeInfluence3 { get => GetFloat("slopeInfluence3"); set { SetFloat("slopeInfluence3", value); } }
        private static float ProceduralTexturing4 { get => GetFloat("proceduralTexturing4"); set { SetFloat("proceduralTexturing4", value); } }
        private static float GradientTexturing4 { get => GetFloat("gradientTexturing4"); set { SetFloat("gradientTexturing4", value); } }
        private static float GradientTiling4 { get => GetFloat("gradientTiling4"); set { SetFloat("gradientTiling4", value); } }
        private static float ColorDamping4 { get => GetFloat("colorDamping4"); set { SetFloat("colorDamping4", value); } }
        private static float SlopeInfluence4 { get => GetFloat("slopeInfluence4"); set { SetFloat("slopeInfluence4", value); } }
        private static float ProceduralTexturing5 { get => GetFloat("proceduralTexturing5"); set { SetFloat("proceduralTexturing5", value); } }
        private static float GradientTexturing5 { get => GetFloat("gradientTexturing5"); set { SetFloat("gradientTexturing5", value); } }
        private static float GradientTiling5 { get => GetFloat("gradientTiling5"); set { SetFloat("gradientTiling5", value); } }
        private static float ColorDamping5 { get => GetFloat("colorDamping5"); set { SetFloat("colorDamping5", value); } }
        private static float SlopeInfluence5 { get => GetFloat("slopeInfluence5"); set { SetFloat("slopeInfluence5", value); } }
        private static float ProceduralTexturing6 { get => GetFloat("proceduralTexturing6"); set { SetFloat("proceduralTexturing6", value); } }
        private static float GradientTexturing6 { get => GetFloat("gradientTexturing6"); set { SetFloat("gradientTexturing6", value); } }
        private static float GradientTiling6 { get => GetFloat("gradientTiling6"); set { SetFloat("gradientTiling6", value); } }
        private static float ColorDamping6 { get => GetFloat("colorDamping6"); set { SetFloat("colorDamping6", value); } }
        private static float SlopeInfluence6 { get => GetFloat("slopeInfluence6"); set { SetFloat("slopeInfluence6", value); } }
        private static float ProceduralTexturing7 { get => GetFloat("proceduralTexturing7"); set { SetFloat("proceduralTexturing7", value); } }
        private static float GradientTexturing7 { get => GetFloat("gradientTexturing7"); set { SetFloat("gradientTexturing7", value); } }
        private static float GradientTiling7 { get => GetFloat("gradientTiling7"); set { SetFloat("gradientTiling7", value); } }
        private static float ColorDamping7 { get => GetFloat("colorDamping7"); set { SetFloat("colorDamping7", value); } }
        private static float SlopeInfluence7 { get => GetFloat("slopeInfluence7"); set { SetFloat("slopeInfluence7", value); } }
        private static float ProceduralTexturing8 { get => GetFloat("proceduralTexturing8"); set { SetFloat("proceduralTexturing8", value); } }
        private static float GradientTexturing8 { get => GetFloat("gradientTexturing8"); set { SetFloat("gradientTexturing8", value); } }
        private static float GradientTiling8 { get => GetFloat("gradientTiling8"); set { SetFloat("gradientTiling8", value); } }
        private static float ColorDamping8 { get => GetFloat("colorDamping8"); set { SetFloat("colorDamping8", value); } }
        private static float SlopeInfluence8 { get => GetFloat("slopeInfluence8"); set { SetFloat("slopeInfluence8", value); } }

        //public static float PuddleNoiseInfluence { get => GetFloat("_NoiseIntensity"); set { SetFloat("_NoiseIntensity", value); } }
        //private static Texture2D noiseTexture;
        //public static Texture2D NoiseTexture { get => GetNoiseTexture(); }

        private static Color GetColorBG(string variable)
        {
            if (TerrainMaterialBG != null && TerrainMaterialBG.HasProperty(variable))
                return TerrainMaterialBG.GetColor(variable);
            else
                return new Color(1, 1, 1);
        }

        private static void SetColorBG(string variable, Color value)
        {
            if (!isModernRendering) return;

            if (TerrainMaterialBG != null && TerrainMaterialBG.HasProperty(variable))
                TerrainMaterialBG.SetColor(variable, value);
        }

        // We need to reverse values to be in sync with UI's 0~100 % range
        private static float TessellationQuality
        {
            get
            {
                if (!isTessellated) return 25;
                float v = GetFloat("_EdgeLength");
                v = (210f / v) - 1f;
                return v;
            }
            set
            {
                if (isTessellated)
                {
                    float v = 210f / (1 + value);
                    SetFloat("_EdgeLength", v);
                }  
            }
        }

        private static float EdgeSmoothness
        {
            get
            {
                if (!isTessellated) return 0;
                return GetFloat("_Phong");
            }
            set
            {
                if (isTessellated)
                    SetFloat("_Phong", value);
            }
        }

        private static float GetFloat(string variable)
        {
            if (isModernRendering && TerrainMaterial.HasProperty(variable))
                return TerrainMaterial.GetFloat(variable);
            else
                return 0;
        }

        private static void SetFloat(string variable, float value)
        {
            if (isModernRendering && TerrainMaterial.HasProperty(variable))
                TerrainMaterial.SetFloat(variable, value);
        }

        private static void SetFloatBG(string variable, float value)
        {
            if (isModernRendering)
            {
                if (TerrainMaterialBG != null && TerrainMaterialBG.HasProperty(variable))
                    TerrainMaterialBG.SetFloat(variable, value);
            }
        }

        private static Color GetColor(string variable)
        {
            if (isModernRendering && TerrainMaterial.HasProperty(variable))
                return TerrainMaterial.GetColor(variable);
            else
                return new Color(1, 1, 1);
        }

        private static void SetColor(string variable, Color value)
        {
            if (isModernRendering && TerrainMaterial.HasProperty(variable))
                TerrainMaterial.SetColor(variable, value);
        }

        private static float GetDisplacement(int index)
        {
            if (!isTessellated) return 0.1f;
            return GetFloat("_Displacement" + (index + 1).ToString());
        }

        private static void SetDisplacement(int index, float value)
        {
            if (isTessellated)
                SetFloat("_Displacement" + (index + 1).ToString(), value);
        }

        private static float GetHeightOffset(int index)
        {
            if (!isTessellated) return 0.1f;
            return GetFloat("_HeightShift" + (index + 1).ToString());
        }

        private static void SetHeightOffset(int index, float value)
        {
            if (isTessellated)
                SetFloat("_HeightShift" + (index + 1).ToString(), value);
        }

        private static float GetTileRemover(int index)
        {
            return GetFloat("_TilingRemover" + (index + 1).ToString());
        }

        private static void SetTileRemover(int index, float value)
        {

            SetFloat("_TilingRemover" + (index + 1).ToString(), value);
        }

        private static float GetNoiseTiling(int index)
        {
            return GetFloat("_NoiseTiling" + (index + 1).ToString());
        }

        private static void SetNoseTiling(int index, float value)
        {
            SetFloat("_NoiseTiling" + (index + 1).ToString(), value);
        }

        private static Color GetLayerColor(int index)
        {
            return GetColor("_LayerColor" + (index + 1).ToString());
        }

        private static void SetLayerColor(int index, Color value)
        {
            SetColor("_LayerColor" + (index + 1).ToString(), value);
        }

        private static float GetLayerAO(int index)
        {
            return GetFloat("_LayerAO" + (index + 1).ToString());
        }

        private static void SetLayerAO(int index, float value)
        {
            SetFloat("_LayerAO" + (index + 1).ToString(), value);
        }

        private static float GetLayerNormalScale(int index)
        {
            if (terrainLayer(index) == null) return 1;
            return terrainLayer(index).normalScale;
        }

        private static void SetLayerNormalScale(int index, float value)
        {
            if (terrainLayer(index) == null) return;
            terrainLayer(index).normalScale = value;
        }

        private static float GetLayerMetallic(int index)
        {
            if (terrainLayer(index) == null) return 0;
            return terrainLayer(index).metallic;
        }

        private static void SetLayerMetallic(int index, float value)
        {
            if (terrainLayer(index) == null) return;
            terrainLayer(index).metallic = value;
        }

        private static float GetLayerSmoothness(int index)
        {
            if (terrainLayer(index) == null) return 0;
            return terrainLayer(index).smoothness;
        }

        private static void SetLayerSmoothness(int index, float value)
        {
            if (terrainLayer(index) == null) return;
            terrainLayer(index).smoothness = value;
        }

        private static Vector4 GetLayerTiling(int index)
        {
            if (terrainLayer(index) == null) return new Vector4(4, 4, 0, 0);
            return new Vector4(terrainLayer(index).tileSize.x, terrainLayer(index).tileSize.y, terrainLayer(index).tileOffset.x, terrainLayer(index).tileOffset.y);
        }

        private static void SetLayerTiling(int index, Vector4 value)
        {
            if (terrainLayer(index) == null) return;
            terrainLayer(index).tileSize = new Vector2(value.x, value.y);
            terrainLayer(index).tileOffset = new Vector2(value.z, value.w);
        }

        private static TerrainLayer terrainLayer (int index)
        {
            if (index >= MainTerrain.terrainData.terrainLayers.Length) return null;
            TerrainLayer tl = MainTerrain.terrainData.terrainLayers[index];
            if (tl == null) throw new Exception("Terrain Layer " + (index + 1) + " not found on terrain!");
            return tl;
        }

        private static Terrain MainTerrain  
        {
            get
            {
                Terrain _targetMainTerrain = null;

                if (TTerraWorldManager.MainTerrainGO == null)
                    _targetMainTerrain = null;
                else
                    _targetMainTerrain = TTerraWorldManager.MainTerrain;

                if (_targetMainTerrain == null)
                    throw new Exception("No Terrains Found!");
                else
                    return _targetMainTerrain;
            }
        }

        private static Terrain BGTerrain
        {
            get
            {
                Terrain _targetBGTerrain = null;

                if (TTerraWorldManager.BackgroundTerrainGO == null)
                    _targetBGTerrain = null;
                else
                    _targetBGTerrain = TTerraWorldManager.BackgroundTerrain;

                if (_targetBGTerrain == null)
                    throw new Exception("No Background Terrain Found!");
                else
                    return _targetBGTerrain;
            }
        }

        private static Material GetTerrainMaterial()
        {
            if (TTerraWorldManager.MainTerrainGO != null)
                return MainTerrain.materialTemplate;
            else
                return null;
        }

        private static Material GetBGTerrainMaterial()
        {
            if (TTerraWorldManager.BackgroundTerrainGO != null)
                return BGTerrain.materialTemplate;
            else
                return null;
        }

        private static bool IsModernRendering()
        {
            TerrainShaderType terrainShaderType = GetTerrainShaderType();

            if (terrainShaderType == TerrainShaderType.TerraFormerInstanced || terrainShaderType == TerrainShaderType.TerraFormer)
                return true;
            else
                return false;
        }

        private static void SetModernRendering(bool isModernRendering)
        {
            if (TerrainMaterial == null) return;

            if (isModernRendering)
            {
                TerrainMaterial.shader = Shader.Find("TerraUnity/TerraFormer Instanced");

#if !UNITY_2019_2_OR_NEWER
                MainTerrain.materialType = Terrain.MaterialType.Custom;
#endif

                MainTerrain.drawInstanced = false;
            }
            else
            {
                TerrainMaterial.shader = Shader.Find("Nature/Terrain/Standard");
                MainTerrain.drawInstanced = true;
            }

            if (TerrainMaterialBG == null) return;

            if (isModernRendering)
            {
                TerrainMaterialBG.shader = Shader.Find("TerraUnity/TerraFormer Instanced");

#if !UNITY_2019_2_OR_NEWER
                BGTerrain.materialType = Terrain.MaterialType.Custom;
#endif

                BGTerrain.drawInstanced = false;
            }
            else
            {
                TerrainMaterialBG.shader = Shader.Find("Nature/Terrain/Standard");
                BGTerrain.drawInstanced = true;
            }
        }

        private static TerrainShaderType GetTerrainShaderType()
        {
            if (TerrainMaterial == null)
                return TerrainShaderType.Other; 
            else if (TerrainMaterial.shader == Shader.Find("TerraUnity/TerraFormer"))
                return TerrainShaderType.TerraFormer;
            else if (TerrainMaterial.shader == Shader.Find("TerraUnity/TerraFormer Instanced"))
                return TerrainShaderType.TerraFormerInstanced;
            else
                return TerrainShaderType.Other;
        }

        private static bool IsTerrainTessellation()
        {
            TerrainShaderType terrainShaderType = GetTerrainShaderType();

            if
            (
                terrainShaderType == TerrainShaderType.TerraFormer
                && TerrainMaterial.IsKeywordEnabled("_TESSELLATION")
                //&& !MainTerrain.drawInstanced
            )
                return true;
            else
                return false;
        }

        private static void SetTerrainTessellation(bool enabled)
        {
            TerrainShaderType terrainShaderType = GetTerrainShaderType();
            if (terrainShaderType == TerrainShaderType.Other) return;

            if (enabled)
            {
                if (terrainShaderType == TerrainShaderType.TerraFormerInstanced)
                    TerrainMaterial.shader = Shader.Find("TerraUnity/TerraFormer");

                TerrainMaterial.EnableKeyword("_TESSELLATION");
            }
            else
            {
                if (terrainShaderType == TerrainShaderType.TerraFormer)
                    TerrainMaterial.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
            }

            MainTerrain.drawInstanced = false;
        }

        private static bool IsTerrainHeightmapBlending()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_HEIGHTMAPBLENDING"))
                return true;
            else
                return false;
        }

        private static void SetTerrainHeightmapBlending(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
                TerrainMaterial.EnableKeyword("_HEIGHTMAPBLENDING");
            else
                TerrainMaterial.DisableKeyword("_HEIGHTMAPBLENDING");
        }

        private static bool IsTerrainColormapBlending()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_COLORMAPBLENDING") && ColormapTexture != null)
                return true;
            else
                return false;
        }

        private static void SetTerrainColormapBlending(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled && ColormapTexture != null)
                TerrainMaterial.EnableKeyword("_COLORMAPBLENDING");
            else
                TerrainMaterial.DisableKeyword("_COLORMAPBLENDING");
        }

        private static bool IsTerrainProceduralSnow()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_PROCEDURALSNOW"))
                return true;
            else
                return false;
        }

        private static void SetTerrainProceduralSnow(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
            {
                TerrainMaterial.EnableKeyword("_PROCEDURALSNOW");
                TerrainMaterial.SetFloat("_SnowState", 1);
            }
            else
            {
                TerrainMaterial.DisableKeyword("_PROCEDURALSNOW");
                TerrainMaterial.SetFloat("_SnowState", 0);
            }

            SetTerrainProceduralSnowBG(enabled);
        }

        private static bool IsTerrainProceduralSnowBG()
        {
            if (TerrainMaterialBG == null) return false;

            if (TerrainMaterialBG.IsKeywordEnabled("_PROCEDURALSNOW"))
                return true;
            else
                return false;
        }

        private static void SetTerrainProceduralSnowBG(bool enabled)
        {
            if (TerrainMaterialBG == null || !isModernRendering) return;

            if (enabled)
            {
                TerrainMaterialBG.EnableKeyword("_PROCEDURALSNOW");
                TerrainMaterialBG.SetFloat("_SnowState", 1);
                TerrainMaterialBG.SetColor("_SnowColor", TerrainMaterial.GetColor("_SnowColor"));
                TerrainMaterialBG.SetFloat("_SnowTile", TerrainMaterial.GetFloat("_SnowTile"));
                TerrainMaterialBG.SetFloat("_SnowAmount", TerrainMaterial.GetFloat("_SnowAmount"));
                TerrainMaterialBG.SetFloat("_SnowAngle", TerrainMaterial.GetFloat("_SnowAngle"));
                TerrainMaterialBG.SetFloat("_NormalInfluence", TerrainMaterial.GetFloat("_NormalInfluence"));
                TerrainMaterialBG.SetFloat("_SnowPower", TerrainMaterial.GetFloat("_SnowPower"));
                TerrainMaterialBG.SetFloat("_SnowSmoothness", TerrainMaterial.GetFloat("_SnowSmoothness"));
                //TerrainMaterialBG.SetFloat("_SnowMetallic", TerrainMaterial.GetFloat("_SnowMetallic"));
            }
            else
            {
                TerrainMaterialBG.DisableKeyword("_PROCEDURALSNOW");
                TerrainMaterialBG.SetFloat("_SnowState", 0);
            }
        }

        private static bool IsTerrainFlatShading()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_FLATSHADING"))
                return true;
            else
                return false;
        }

        private static void SetTerrainFlatShading(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
            {
                if (TerrainMaterial != null)
                {
                    TerrainMaterial.EnableKeyword("_FLATSHADING");
                    TerrainMaterial.SetFloat("_FlatShadingState", 1);
                }
            }
            else
            {
                if (TerrainMaterial != null)
                {
                    TerrainMaterial.DisableKeyword("_FLATSHADING");
                    TerrainMaterial.SetFloat("_FlatShadingState", 0);
                }
            }

            SetTerrainFlatShadingBG(enabled);
        }

        private static void SetTerrainFlatShadingBG(bool enabled)
        {
            if (TerrainMaterialBG == null) return;

            if (enabled)
            {
                TerrainMaterialBG.EnableKeyword("_FLATSHADING");
                if (TerrainMaterialBG.HasProperty("_FlatShadingState")) TerrainMaterialBG.SetFloat("_FlatShadingState", 1);
            }
            else
            {
                TerrainMaterialBG.DisableKeyword("_FLATSHADING");
                if (TerrainMaterialBG.HasProperty("_FlatShadingState")) TerrainMaterialBG.SetFloat("_FlatShadingState", 0);
            }
        }

        private static bool IsTerrainProceduralPuddles()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_PROCEDURALPUDDLES"))
                return true;
            else
                return false;
        }

        private static void SetTerrainProceduralPuddles(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
                TerrainMaterial.EnableKeyword("_PROCEDURALPUDDLES");
            else
                TerrainMaterial.DisableKeyword("_PROCEDURALPUDDLES");
        }

        private static Texture2D GetColormapTexture()
        {
            if (!isModernRendering) colormapTexture = null;

            if (colormapTexture == null)
                colormapTexture = TerrainMaterial.GetTexture("_ColorMap") as Texture2D;

            if (colormapTexture == null)
                TerrainMaterial.DisableKeyword("_COLORMAPBLENDING");

            return colormapTexture;
        }

        private static void SetColormapTexture(Texture2D texture2D)
        {
            colormapTexture = texture2D;
            TerrainMaterial.SetTexture("_ColorMap", texture2D);

            if (colormapTexture == null)
                TerrainMaterial.DisableKeyword("_COLORMAPBLENDING");
        }

        private static Texture2D GetWaterMaskTexture()
        {
            if (!isModernRendering) waterMaskTexture = null;

            //if (waterMaskTexture == null)
            waterMaskTexture = TerrainMaterial.GetTexture("_WaterMask") as Texture2D;

            return waterMaskTexture;
        }

        private static void SetWaterMaskTexture(Texture2D _waterMask)
        {
            waterMaskTexture = _waterMask;
            TerrainMaterial.SetTexture("_WaterMask", _waterMask);
        }

        private static Texture2D GetSnowTexture()
        {
            if (!isModernRendering) snowTexture = null;
            else snowTexture = TerrainMaterial.GetTexture("_SnowDiffuse") as Texture2D;
            return snowTexture;
        }

        private static int GetTerrainLayersCount()
        {
            if (MainTerrain != null && MainTerrain.terrainData != null && MainTerrain.terrainData.terrainLayers != null)
                terrainLayersCount = MainTerrain.terrainData.terrainLayers.Length;
            else
                terrainLayersCount = 0;

            return terrainLayersCount;
        }

        private static float SnowThickness { get => GetFloat("_SnowThickness"); set { SetFloat("_SnowThickness", value); SetFloatBG("_SnowThickness", value); } }
        private static float SnowDamping { get => GetFloat("_SnowDamping"); set { SetFloat("_SnowDamping", value); SetFloatBG("_SnowDamping", value); } }
        public static int TerrainLayersCount { get => GetTerrainLayersCount(); }
        public static Texture2D SnowTexture { get => GetSnowTexture(); }
        public static Texture2D WaterMaskTexture { get => GetWaterMaskTexture(); set => SetWaterMaskTexture(value); }
        public static Material TerrainMaterial { get => GetTerrainMaterial(); }
        public static Material TerrainMaterialBG { get => GetBGTerrainMaterial(); }

        //private static TerrainRenderingParams _parameters = new TerrainRenderingParams(true);

        public static TerrainRenderingParams GetParams()
        {
            TerrainRenderingParams _parameters = TTerraWorldManager.WorldGraph.RenderingDATA;

            if (TerrainMaterial != null)
            {
                _parameters.modernRendering = isModernRendering;

                if (_parameters.modernRendering)
                {
                    _parameters.tessellation = isTessellated;
                    _parameters.heightmapBlending = isHeightmapBlending;
                    _parameters.colormapBlending = isColormapBlending;
                    //_parameters.proceduralSnow = isProceduralSnow;
                    _parameters.proceduralPuddles = isProceduralPuddles;
                    _parameters.isFlatShading = isFlatShading;
                    _parameters.surfaceTintColorMAIN = TUtils.UnityColorToVector4(LightingColor);
                    _parameters.surfaceTintColorBG = TUtils.UnityColorToVector4(LightingColorBG);

                    // Only get following params if Tessellation is enabled
                    if (_parameters.tessellation)
                    {
                        _parameters.tessellationQuality = TessellationQuality;
                        _parameters.edgeSmoothness = EdgeSmoothness;
                        _parameters.displacement1 = GetDisplacement(0);
                        _parameters.displacement2 = GetDisplacement(1);
                        _parameters.displacement3 = GetDisplacement(2);
                        _parameters.displacement4 = GetDisplacement(3);
                        _parameters.displacement5 = GetDisplacement(4);
                        _parameters.displacement6 = GetDisplacement(5);
                        _parameters.displacement7 = GetDisplacement(6);
                        _parameters.displacement8 = GetDisplacement(7);
                        _parameters.heightOffset1 = GetHeightOffset(0);
                        _parameters.heightOffset2 = GetHeightOffset(1);
                        _parameters.heightOffset3 = GetHeightOffset(2);
                        _parameters.heightOffset4 = GetHeightOffset(3);
                        _parameters.heightOffset5 = GetHeightOffset(4);
                        _parameters.heightOffset6 = GetHeightOffset(5);
                        _parameters.heightOffset7 = GetHeightOffset(6);
                        _parameters.heightOffset8 = GetHeightOffset(7);
                    }

                    _parameters.heightBlending = HeightmapBlending;

                    _parameters.tilingRemover1 = GetTileRemover(0);
                    _parameters.tilingRemover2 = GetTileRemover(1);
                    _parameters.tilingRemover3 = GetTileRemover(2);
                    _parameters.tilingRemover4 = GetTileRemover(3);
                    _parameters.tilingRemover5 = GetTileRemover(4);
                    _parameters.tilingRemover6 = GetTileRemover(5);
                    _parameters.tilingRemover7 = GetTileRemover(6);
                    _parameters.tilingRemover8 = GetTileRemover(7);

                    _parameters.noiseTiling1 = GetNoiseTiling(0);
                    _parameters.noiseTiling2 = GetNoiseTiling(1);
                    _parameters.noiseTiling3 = GetNoiseTiling(2);
                    _parameters.noiseTiling4 = GetNoiseTiling(3);
                    _parameters.noiseTiling5 = GetNoiseTiling(4);
                    _parameters.noiseTiling6 = GetNoiseTiling(5);
                    _parameters.noiseTiling7 = GetNoiseTiling(6);
                    _parameters.noiseTiling8 = GetNoiseTiling(7);

                    _parameters.colormapBlendingDistance = ColormapBlendingDistance;

                    _parameters.snowColorR = SnowColor.r;
                    _parameters.snowColorG = SnowColor.g;
                    _parameters.snowColorB = SnowColor.b;
                    _parameters.snowTiling = SnowTile;
                    _parameters.snowAmount = SnowAmount;
                    _parameters.snowAngles = SnowAngle;
                    _parameters.snowNormalInfluence = SnowNormalInfluence;
                    _parameters.snowPower = SnowPower;
                    _parameters.snowSmoothness = SnowSmoothness;
                    _parameters.snowStartHeight = SnowStartHeight;
                    _parameters.heightFalloff = HeightFalloff;
                    _parameters.snowThickness = SnowThickness;
                    _parameters.snowdamping = SnowDamping;

                    _parameters.puddleColorR = PuddleColor.r;
                    _parameters.puddleColorG = PuddleColor.g;
                    _parameters.puddleColorB = PuddleColor.b;
                    _parameters.puddleRefraction = PuddleRefraction;
                    _parameters.puddleMetallic = PuddleMetallic;
                    _parameters.puddleSmoothness = PuddleSmoothness;
                    _parameters.puddlewaterHeight = PuddlewaterHeight;
                    _parameters.puddleSlope = PuddleSlope;
                    _parameters.puddleMinSlope = PuddleSlopeMin;
                    _parameters.puddleNoiseTiling = PuddleNoiseTiling;
                    //_parameters.puddleNoiseInfluence = PuddleNoiseInfluence;
                    //_parameters.puddleReflections = PuddleReflections;

                    _parameters.layerColor1R = GetLayerColor(0).r;
                    _parameters.layerColor1G = GetLayerColor(0).g;
                    _parameters.layerColor1B = GetLayerColor(0).b;
                    _parameters.layerColor2R = GetLayerColor(1).r;
                    _parameters.layerColor2G = GetLayerColor(1).g;
                    _parameters.layerColor2B = GetLayerColor(1).b;
                    _parameters.layerColor3R = GetLayerColor(2).r;
                    _parameters.layerColor3G = GetLayerColor(2).g;
                    _parameters.layerColor3B = GetLayerColor(2).b;
                    _parameters.layerColor4R = GetLayerColor(3).r;
                    _parameters.layerColor4G = GetLayerColor(3).g;
                    _parameters.layerColor4B = GetLayerColor(3).b;
                    _parameters.layerColor5R = GetLayerColor(4).r;
                    _parameters.layerColor5G = GetLayerColor(4).g;
                    _parameters.layerColor5B = GetLayerColor(4).b;
                    _parameters.layerColor6R = GetLayerColor(5).r;
                    _parameters.layerColor6G = GetLayerColor(5).g;
                    _parameters.layerColor6B = GetLayerColor(5).b;
                    _parameters.layerColor7R = GetLayerColor(6).r;
                    _parameters.layerColor7G = GetLayerColor(6).g;
                    _parameters.layerColor7B = GetLayerColor(6).b;
                    _parameters.layerColor8R = GetLayerColor(7).r;
                    _parameters.layerColor8G = GetLayerColor(7).g;
                    _parameters.layerColor8B = GetLayerColor(7).b;
                    _parameters.layerAO1 = GetLayerAO(0);
                    _parameters.layerAO2 = GetLayerAO(1);
                    _parameters.layerAO3 = GetLayerAO(2);
                    _parameters.layerAO4 = GetLayerAO(3);
                    _parameters.layerAO5 = GetLayerAO(4);
                    _parameters.layerAO6 = GetLayerAO(5);
                    _parameters.layerAO7 = GetLayerAO(6);
                    _parameters.layerAO8 = GetLayerAO(7);
                    //_parameters.layerNormalScale1 = GetLayerNormalScale(0);
                    //_parameters.layerMetallic1 = GetLayerMetallic(0);
                    //_parameters.layerSmoothness1 = GetLayerSmoothness(0);
                    //_parameters.layerTiling1 = GetLayerTiling(0);
                    //_parameters.layerNormalScale2 = GetLayerNormalScale(1);
                    //_parameters.layerMetallic2 = GetLayerMetallic(1);
                    //_parameters.layerSmoothness2 = GetLayerSmoothness(1);
                    //_parameters.layerTiling2 = GetLayerTiling(1);
                    //_parameters.layerNormalScale3 = GetLayerNormalScale(2);
                    //_parameters.layerMetallic3 = GetLayerMetallic(2);
                    //_parameters.layerSmoothness3 = GetLayerSmoothness(2);
                    //_parameters.layerTiling3 = GetLayerTiling(2);
                    //_parameters.layerNormalScale4 = GetLayerNormalScale(3);
                    //_parameters.layerMetallic4 = GetLayerMetallic(3);
                    //_parameters.layerSmoothness4 = GetLayerSmoothness(3);
                    //_parameters.layerTiling4 = GetLayerTiling(3);
                    //_parameters.layerNormalScale5 = GetLayerNormalScale(4);
                    //_parameters.layerMetallic5 = GetLayerMetallic(4);
                    //_parameters.layerSmoothness5 = GetLayerSmoothness(4);
                    //_parameters.layerTiling5 = GetLayerTiling(4);
                    //_parameters.layerNormalScale6 = GetLayerNormalScale(5);
                    //_parameters.layerMetallic6 = GetLayerMetallic(5);
                    //_parameters.layerSmoothness6 = GetLayerSmoothness(5);
                    //_parameters.layerTiling6 = GetLayerTiling(5);
                    //_parameters.layerNormalScale7 = GetLayerNormalScale(6);
                    //_parameters.layerMetallic7 = GetLayerMetallic(6);
                    //_parameters.layerSmoothness7 = GetLayerSmoothness(6);
                    //_parameters.layerTiling7 = GetLayerTiling(6);
                    //_parameters.layerNormalScale8 = GetLayerNormalScale(7);
                    //_parameters.layerMetallic8 = GetLayerMetallic(7);
                    //_parameters.layerSmoothness8 = GetLayerSmoothness(7);
                    //_parameters.layerTiling8 = GetLayerTiling(7);

                    // Global Texturing
                    _parameters.proceduralNoiseScale = ProceduralNoiseScale;
                    _parameters.proceduralTexturing1 = Convert.ToBoolean(ProceduralTexturing1);
                    _parameters.gradientTexturing1 = Convert.ToBoolean(GradientTexturing1);
                    _parameters.gradientTiling1 = GradientTiling1;
                    _parameters.colorDamping1 = ColorDamping1;
                    _parameters.slopeInfluence1 = SlopeInfluence1;
                    _parameters.proceduralTexturing2 = Convert.ToBoolean(ProceduralTexturing2);
                    _parameters.gradientTexturing2 = Convert.ToBoolean(GradientTexturing2);
                    _parameters.gradientTiling2 = GradientTiling2;
                    _parameters.colorDamping2 = ColorDamping2;
                    _parameters.slopeInfluence2 = SlopeInfluence2;
                    _parameters.proceduralTexturing3 = Convert.ToBoolean(ProceduralTexturing3);
                    _parameters.gradientTexturing3 = Convert.ToBoolean(GradientTexturing3);
                    _parameters.gradientTiling3 = GradientTiling3;
                    _parameters.colorDamping3 = ColorDamping3;
                    _parameters.slopeInfluence3 = SlopeInfluence3;
                    _parameters.proceduralTexturing4 = Convert.ToBoolean(ProceduralTexturing4);
                    _parameters.gradientTexturing4 = Convert.ToBoolean(GradientTexturing4);
                    _parameters.gradientTiling4 = GradientTiling4;
                    _parameters.colorDamping4 = ColorDamping4;
                    _parameters.slopeInfluence4 = SlopeInfluence4;

                    _parameters.proceduralTexturing5 = Convert.ToBoolean(ProceduralTexturing5);
                    _parameters.gradientTexturing5 = Convert.ToBoolean(GradientTexturing5);
                    _parameters.gradientTiling5 = GradientTiling5;
                    _parameters.colorDamping5 = ColorDamping5;
                    _parameters.slopeInfluence5 = SlopeInfluence5;
                    _parameters.proceduralTexturing6 = Convert.ToBoolean(ProceduralTexturing6);
                    _parameters.gradientTexturing6 = Convert.ToBoolean(GradientTexturing6);
                    _parameters.gradientTiling6 = GradientTiling6;
                    _parameters.colorDamping6 = ColorDamping6;
                    _parameters.slopeInfluence6 = SlopeInfluence6;
                    _parameters.proceduralTexturing7 = Convert.ToBoolean(ProceduralTexturing7);
                    _parameters.gradientTexturing7 = Convert.ToBoolean(GradientTexturing7);
                    _parameters.gradientTiling7 = GradientTiling7;
                    _parameters.colorDamping7 = ColorDamping7;
                    _parameters.slopeInfluence7 = SlopeInfluence7;
                    _parameters.proceduralTexturing8 = Convert.ToBoolean(ProceduralTexturing8);
                    _parameters.gradientTexturing8 = Convert.ToBoolean(GradientTexturing8);
                    _parameters.gradientTiling8 = GradientTiling8;
                    _parameters.colorDamping8 = ColorDamping8;
                    _parameters.slopeInfluence8 = SlopeInfluence8;
                }
            }

            TTerraWorldManager.WorldGraph.RenderingDATA = _parameters;
            return TTerraWorldManager.WorldGraph.RenderingDATA;
        }

        public static void SetParams(TerrainRenderingParams renderingParams)
        {
            TTerraWorldManager.WorldGraph.RenderingDATA = renderingParams;
            Apply();
            TTerraWorldManager.SaveOldGraph();
        }

        private static void Apply()
        {
            TerrainRenderingParams _parameters = TTerraWorldManager.WorldGraph.RenderingDATA;

            if (TerrainMaterial != null)
                isModernRendering = _parameters.modernRendering;

            if (_parameters.modernRendering)
            {
                //WorldName = _parameters.worldName;
                isTessellated = _parameters.tessellation;
                isHeightmapBlending = _parameters.heightmapBlending;
                isColormapBlending = _parameters.colormapBlending;
                //isProceduralSnow = _parameters.proceduralSnow;
                isProceduralPuddles = _parameters.proceduralPuddles;
                isFlatShading = _parameters.isFlatShading;

                LightingColor = TUtils.Vector4ToUnityColor(_parameters.surfaceTintColorMAIN);

                if (TTerraWorldManager.BackgroundTerrainGO != null)
                    LightingColorBG = TUtils.Vector4ToUnityColor(_parameters.surfaceTintColorBG);

                // Only set following params if Tessellation is enabled
                if (_parameters.tessellation)
                {
                    TessellationQuality = _parameters.tessellationQuality;
                    EdgeSmoothness = _parameters.edgeSmoothness;
                    SetDisplacement(0, _parameters.displacement1);
                    SetDisplacement(1, _parameters.displacement2);
                    SetDisplacement(2, _parameters.displacement3);
                    SetDisplacement(3, _parameters.displacement4);
                    SetDisplacement(4, _parameters.displacement5);
                    SetDisplacement(5, _parameters.displacement6);
                    SetDisplacement(6, _parameters.displacement7);
                    SetDisplacement(7, _parameters.displacement8);
                    SetHeightOffset(0, _parameters.heightOffset1);
                    SetHeightOffset(1, _parameters.heightOffset2);
                    SetHeightOffset(2, _parameters.heightOffset3);
                    SetHeightOffset(3, _parameters.heightOffset4);
                    SetHeightOffset(4, _parameters.heightOffset5);
                    SetHeightOffset(5, _parameters.heightOffset6);
                    SetHeightOffset(6, _parameters.heightOffset7);
                    SetHeightOffset(7, _parameters.heightOffset8);
                }

                HeightmapBlending = _parameters.heightBlending;

                SetTileRemover(0, _parameters.tilingRemover1);
                SetTileRemover(1, _parameters.tilingRemover2);
                SetTileRemover(2, _parameters.tilingRemover3);
                SetTileRemover(3, _parameters.tilingRemover4);
                SetTileRemover(4, _parameters.tilingRemover5);
                SetTileRemover(5, _parameters.tilingRemover6);
                SetTileRemover(6, _parameters.tilingRemover7);
                SetTileRemover(7, _parameters.tilingRemover8);

                SetNoseTiling(0, _parameters.noiseTiling1);
                SetNoseTiling(1, _parameters.noiseTiling2);
                SetNoseTiling(2, _parameters.noiseTiling3);
                SetNoseTiling(3, _parameters.noiseTiling4);
                SetNoseTiling(4, _parameters.noiseTiling5);
                SetNoseTiling(5, _parameters.noiseTiling6);
                SetNoseTiling(6, _parameters.noiseTiling7);
                SetNoseTiling(7, _parameters.noiseTiling8);

                ColormapBlendingDistance = _parameters.colormapBlendingDistance;

                SnowColor = new Color(_parameters.snowColorR, _parameters.snowColorG, _parameters.snowColorB);

                SnowTile = _parameters.snowTiling;
                SnowAmount = _parameters.snowAmount;
                SnowAngle = _parameters.snowAngles;
                SnowNormalInfluence = _parameters.snowNormalInfluence;
                SnowPower = _parameters.snowPower;
                SnowSmoothness = _parameters.snowSmoothness;
                SnowStartHeight = _parameters.snowStartHeight;
                HeightFalloff = _parameters.heightFalloff;
                SnowThickness = _parameters.snowThickness;
                SnowDamping = _parameters.snowdamping;

                PuddleColor = new Color(_parameters.puddleColorR, _parameters.puddleColorG, _parameters.puddleColorB);
                PuddleRefraction = _parameters.puddleRefraction;
                PuddleMetallic = _parameters.puddleMetallic;
                PuddleSmoothness = _parameters.puddleSmoothness;
                PuddlewaterHeight = _parameters.puddlewaterHeight;
                PuddleSlope = _parameters.puddleSlope;
                PuddleSlopeMin = _parameters.puddleMinSlope;
                PuddleNoiseTiling = _parameters.puddleNoiseTiling;
                //PuddleNoiseInfluence = _parameters.puddleNoiseInfluence;
                //PuddleReflections = _parameters.puddleReflections;

                SetLayerColor(0, new Color(_parameters.layerColor1R, _parameters.layerColor1G, _parameters.layerColor1B));
                SetLayerColor(1, new Color(_parameters.layerColor2R, _parameters.layerColor2G, _parameters.layerColor2B));
                SetLayerColor(2, new Color(_parameters.layerColor3R, _parameters.layerColor3G, _parameters.layerColor3B));
                SetLayerColor(3, new Color(_parameters.layerColor4R, _parameters.layerColor4G, _parameters.layerColor4B));
                SetLayerColor(4, new Color(_parameters.layerColor5R, _parameters.layerColor5G, _parameters.layerColor5B));
                SetLayerColor(5, new Color(_parameters.layerColor6R, _parameters.layerColor6G, _parameters.layerColor6B));
                SetLayerColor(6, new Color(_parameters.layerColor7R, _parameters.layerColor7G, _parameters.layerColor7B));
                SetLayerColor(7, new Color(_parameters.layerColor8R, _parameters.layerColor8G, _parameters.layerColor8B));

                SetLayerAO(0, _parameters.layerAO1);
                SetLayerAO(1, _parameters.layerAO2);
                SetLayerAO(2, _parameters.layerAO3);
                SetLayerAO(3, _parameters.layerAO4);
                SetLayerAO(4, _parameters.layerAO5);
                SetLayerAO(5, _parameters.layerAO6);
                SetLayerAO(6, _parameters.layerAO7);
                SetLayerAO(7, _parameters.layerAO8);

                //SetLayerNormalScale(0, _parameters.layerNormalScale1);
                //SetLayerMetallic(0, _parameters.layerMetallic1);
                //SetLayerSmoothness(0, _parameters.layerSmoothness1);
                //SetLayerTiling(0, _parameters.layerTiling1);
                //SetLayerNormalScale(1, _parameters.layerNormalScale2);
                //SetLayerMetallic(1, _parameters.layerMetallic2);
                //SetLayerSmoothness(1, _parameters.layerSmoothness2);
                //SetLayerTiling(1, _parameters.layerTiling2);
                //SetLayerNormalScale(2, _parameters.layerNormalScale3);
                //SetLayerMetallic(2, _parameters.layerMetallic3);
                //SetLayerSmoothness(2, _parameters.layerSmoothness3);
                //SetLayerTiling(2, _parameters.layerTiling3);
                //SetLayerNormalScale(3, _parameters.layerNormalScale4);
                //SetLayerMetallic(3, _parameters.layerMetallic4);
                //SetLayerSmoothness(3, _parameters.layerSmoothness4);
                //SetLayerTiling(3, _parameters.layerTiling4);
                //SetLayerNormalScale(4, _parameters.layerNormalScale5);
                //SetLayerMetallic(4, _parameters.layerMetallic5);
                //SetLayerSmoothness(4, _parameters.layerSmoothness5);
                //SetLayerTiling(4, _parameters.layerTiling5);
                //SetLayerNormalScale(5, _parameters.layerNormalScale6);
                //SetLayerMetallic(5, _parameters.layerMetallic6);
                //SetLayerSmoothness(5, _parameters.layerSmoothness6);
                //SetLayerTiling(5, _parameters.layerTiling6);
                //SetLayerNormalScale(6, _parameters.layerNormalScale7);
                //SetLayerMetallic(6, _parameters.layerMetallic7);
                //SetLayerSmoothness(6, _parameters.layerSmoothness7);
                //SetLayerTiling(6, _parameters.layerTiling7);
                //SetLayerNormalScale(7, _parameters.layerNormalScale8);
                //SetLayerMetallic(7, _parameters.layerMetallic8);
                //SetLayerSmoothness(7, _parameters.layerSmoothness8);
                //SetLayerTiling(7, _parameters.layerTiling8);

                // Global Texturing
                ProceduralNoiseScale = _parameters.proceduralNoiseScale;
                ProceduralTexturing1 = Convert.ToInt32(_parameters.proceduralTexturing1);
                GradientTexturing1 = Convert.ToInt32(_parameters.gradientTexturing1);
                GradientTiling1 = _parameters.gradientTiling1;
                ColorDamping1 = _parameters.colorDamping1;
                SlopeInfluence1 = _parameters.slopeInfluence1;
                ProceduralTexturing2 = Convert.ToInt32(_parameters.proceduralTexturing2);
                GradientTexturing2 = Convert.ToInt32(_parameters.gradientTexturing2);
                GradientTiling2 = _parameters.gradientTiling2;
                ColorDamping2 = _parameters.colorDamping2;
                SlopeInfluence2 = _parameters.slopeInfluence2;
                ProceduralTexturing3 = Convert.ToInt32(_parameters.proceduralTexturing3);
                GradientTexturing3 = Convert.ToInt32(_parameters.gradientTexturing3);
                GradientTiling3 = _parameters.gradientTiling3;
                ColorDamping3 = _parameters.colorDamping3;
                SlopeInfluence3 = _parameters.slopeInfluence3;
                ProceduralTexturing4 = Convert.ToInt32(_parameters.proceduralTexturing4);
                GradientTexturing4 = Convert.ToInt32(_parameters.gradientTexturing4);
                GradientTiling4 = _parameters.gradientTiling4;
                ColorDamping4 = _parameters.colorDamping4;
                SlopeInfluence4 = _parameters.slopeInfluence4;
                ProceduralTexturing5 = Convert.ToInt32(_parameters.proceduralTexturing5);
                GradientTexturing5 = Convert.ToInt32(_parameters.gradientTexturing5);
                GradientTiling5 = _parameters.gradientTiling5;
                ColorDamping5 = _parameters.colorDamping5;
                SlopeInfluence5 = _parameters.slopeInfluence5;
                ProceduralTexturing6 = Convert.ToInt32(_parameters.proceduralTexturing6);
                GradientTexturing6 = Convert.ToInt32(_parameters.gradientTexturing6);
                GradientTiling6 = _parameters.gradientTiling6;
                ColorDamping6 = _parameters.colorDamping6;
                SlopeInfluence6 = _parameters.slopeInfluence6;
                ProceduralTexturing7 = Convert.ToInt32(_parameters.proceduralTexturing7);
                GradientTexturing7 = Convert.ToInt32(_parameters.gradientTexturing7);
                GradientTiling7 = _parameters.gradientTiling7;
                ColorDamping7 = _parameters.colorDamping7;
                SlopeInfluence7 = _parameters.slopeInfluence7;
                ProceduralTexturing8 = Convert.ToInt32(_parameters.proceduralTexturing8);
                GradientTexturing8 = Convert.ToInt32(_parameters.gradientTexturing8);
                GradientTiling8 = _parameters.gradientTiling8;
                ColorDamping8 = _parameters.colorDamping8;
                SlopeInfluence8 = _parameters.slopeInfluence8;
            }
        }

        public static void SwitchTerrainLayer(int replaceIndex, TerrainLayer terrainLayer)
        {
            if (MainTerrain != null && MainTerrain.terrainData != null && MainTerrain.terrainData.terrainLayers != null)
            {
                TerrainLayer[] layers = MainTerrain.terrainData.terrainLayers;

                if (replaceIndex < layers.Length)
                {
                    layers[replaceIndex] = terrainLayer;
                    MainTerrain.terrainData.terrainLayers = layers;
                    MainTerrain.Flush();
                }
            }
        }

        public static TerrainLayer GetTerrainLayer(int Index)
        {
            if (MainTerrain == null)
                throw new Exception("No Terrain Detected");

            if (MainTerrain.terrainData == null)
                throw new Exception("No Terrain Data Detected");

            if (MainTerrain.terrainData.terrainLayers == null)
                throw new Exception("No Terrain Layer Detected");

            if ((Index+1) > MainTerrain.terrainData.terrainLayers.Length)
                throw new Exception("Terrain layer index out of range");

            return MainTerrain.terrainData.terrainLayers[Index];
        }

        public static void SetTerrainMaterialMAINDefault(Terrain _terrain)
        {
            TDebug.TraceMessage();

            if (_terrain == null)
                throw new Exception("No Terrain Component Found! (SetTerrainMaterialMAIN)");

#if !UNITY_2019_2_OR_NEWER
            _terrain.materialType = Terrain.MaterialType.Custom;
#endif
            
            Material mat = _terrain.materialTemplate;
            //if (mat != null) return;
            string materialPath = TTerraWorld.WorkDirectoryLocalPath + "Terrain.mat";

            if (!File.Exists(materialPath))
            {
                TResourcesManager.LoadAllResources();
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.TerraFormerMaterial), materialPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            mat = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            mat.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
            mat.SetTexture("_SnowDiffuse", TResourcesManager.snowAlbedo);
            mat.SetTexture("_SnowNormalmap", TResourcesManager.snowNormalmap);
            mat.SetTexture("_SnowMaskmap", TResourcesManager.snowMaskmap);
            mat.SetTexture("_Noise", TResourcesManager.noise);
            _terrain.materialTemplate = mat;
        }

        public static void SetTerrainMaterialBGDefault(Terrain _terrain)
        {
            TDebug.TraceMessage();

            if (_terrain == null)
                throw new Exception("No Terrain Component Found! (SetTerrainMaterialBG)");

#if !UNITY_2019_2_OR_NEWER
            _terrain.materialType = Terrain.MaterialType.Custom;
#endif
            
            Material mat = _terrain.materialTemplate;
            //if (mat != null) return;
            string materialPath = TTerraWorld.WorkDirectoryLocalPath + "BGTerrain.mat";

            if (!File.Exists(materialPath))
            {
                TResourcesManager.LoadAllResources();
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.TerraFormerMaterialBG), materialPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            mat = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            mat.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
            mat.SetTexture("_SnowDiffuse", TResourcesManager.snowAlbedo);
            mat.SetTexture("_SnowNormalmap", TResourcesManager.snowNormalmap);
            mat.SetTexture("_SnowMaskmap", TResourcesManager.snowMaskmap);
            mat.SetTexture("_Noise", TResourcesManager.noise);
            _terrain.materialTemplate = mat;
        }

        public static void Reset()
        {
            SetParams(new TerrainRenderingParams(true));
        }

        // UI Requests
        //-----------------------------------------------------------------------

        public static Color GetLayerColorUI(ref TerrainRenderingParams renderingParams, int index)
        {
            Color result = new Color(0, 0, 0);

            if (index == 0)
            {
                result.r = renderingParams.layerColor1R;
                result.g = renderingParams.layerColor1G;
                result.b = renderingParams.layerColor1B;
            }
            if (index == 1)
            {
                result.r = renderingParams.layerColor2R;
                result.g = renderingParams.layerColor2G;
                result.b = renderingParams.layerColor2B;
            }
            if (index == 2)
            {
                result.r = renderingParams.layerColor3R;
                result.g = renderingParams.layerColor3G;
                result.b = renderingParams.layerColor3B;
            }
            if (index == 3)
            {
                result.r = renderingParams.layerColor4R;
                result.g = renderingParams.layerColor4G;
                result.b = renderingParams.layerColor4B;
            }
            if (index == 4)
            {
                result.r = renderingParams.layerColor5R;
                result.g = renderingParams.layerColor5G;
                result.b = renderingParams.layerColor5B;
            }
            if (index == 5)
            {
                result.r = renderingParams.layerColor6R;
                result.g = renderingParams.layerColor6G;
                result.b = renderingParams.layerColor6B;
            }
            if (index == 6)
            {
                result.r = renderingParams.layerColor7R;
                result.g = renderingParams.layerColor7G;
                result.b = renderingParams.layerColor7B;
            }
            if (index == 7)
            {
                result.r = renderingParams.layerColor8R;
                result.g = renderingParams.layerColor8G;
                result.b = renderingParams.layerColor8B;
            }

            return result;
        }

        public static void SetLayerColorUI(ref TerrainRenderingParams renderingParams, int index, Color value)
        {
            if (index == 0)
            {
                renderingParams.layerColor1R = value.r;
                renderingParams.layerColor1G = value.g;
                renderingParams.layerColor1B = value.b;
            }
            if (index == 1)
            {
                renderingParams.layerColor2R = value.r;
                renderingParams.layerColor2G = value.g;
                renderingParams.layerColor2B = value.b;
            }
            if (index == 2)
            {
                renderingParams.layerColor3R = value.r;
                renderingParams.layerColor3G = value.g;
                renderingParams.layerColor3B = value.b;
            }
            if (index == 3)
            {
                renderingParams.layerColor4R = value.r;
                renderingParams.layerColor4G = value.g;
                renderingParams.layerColor4B = value.b;
            }
            if (index == 4)
            {
                renderingParams.layerColor5R = value.r;
                renderingParams.layerColor5G = value.g;
                renderingParams.layerColor5B = value.b;
            }
            if (index == 5)
            {
                renderingParams.layerColor6R = value.r;
                renderingParams.layerColor6G = value.g;
                renderingParams.layerColor6B = value.b;
            }
            if (index == 6)
            {
                renderingParams.layerColor7R = value.r;
                renderingParams.layerColor7G = value.g;
                renderingParams.layerColor7B = value.b;
            }
            if (index == 7)
            {
                renderingParams.layerColor8R = value.r;
                renderingParams.layerColor8G = value.g;
                renderingParams.layerColor8B = value.b;
            }
        }

        public static float GetNoiseTilingUI(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.noiseTiling1;
            if (index == 1) return renderingParams.noiseTiling2;
            if (index == 2) return renderingParams.noiseTiling3;
            if (index == 3) return renderingParams.noiseTiling4;
            if (index == 4) return renderingParams.noiseTiling5;
            if (index == 5) return renderingParams.noiseTiling6;
            if (index == 6) return renderingParams.noiseTiling7;
            if (index == 7) return renderingParams.noiseTiling8;

            return 0;
        }

        public static void SetNoseTilingUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.noiseTiling1 = value;
            if (index == 1) renderingParams.noiseTiling2 = value;
            if (index == 2) renderingParams.noiseTiling3 = value;
            if (index == 3) renderingParams.noiseTiling4 = value;
            if (index == 4) renderingParams.noiseTiling5 = value;
            if (index == 5) renderingParams.noiseTiling6 = value;
            if (index == 6) renderingParams.noiseTiling7 = value;
            if (index == 7) renderingParams.noiseTiling8 = value;
        }

        public static float GetTileRemoverUI(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.tilingRemover1;
            if (index == 1) return renderingParams.tilingRemover2;
            if (index == 2) return renderingParams.tilingRemover3;
            if (index == 3) return renderingParams.tilingRemover4;
            if (index == 4) return renderingParams.tilingRemover5;
            if (index == 5) return renderingParams.tilingRemover6;
            if (index == 6) return renderingParams.tilingRemover7;
            if (index == 7) return renderingParams.tilingRemover8;

            return 0;
        }

        public static void SetTileRemoverUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.tilingRemover1 = value;
            if (index == 1) renderingParams.tilingRemover2 = value;
            if (index == 2) renderingParams.tilingRemover3 = value;
            if (index == 3) renderingParams.tilingRemover4 = value;
            if (index == 4) renderingParams.tilingRemover5 = value;
            if (index == 5) renderingParams.tilingRemover6 = value;
            if (index == 6) renderingParams.tilingRemover7 = value;
            if (index == 7) renderingParams.tilingRemover8 = value;
        }

        public static float GetDisplacementUI(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.displacement1;
            if (index == 1) return renderingParams.displacement2;
            if (index == 2) return renderingParams.displacement3;
            if (index == 3) return renderingParams.displacement4;
            if (index == 4) return renderingParams.displacement5;
            if (index == 5) return renderingParams.displacement6;
            if (index == 6) return renderingParams.displacement7;
            if (index == 7) return renderingParams.displacement8;

            return 0;
        }

        public static void SetHeightOffsetUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.heightOffset1 = value;
            if (index == 1) renderingParams.heightOffset2 = value;
            if (index == 2) renderingParams.heightOffset3 = value;
            if (index == 3) renderingParams.heightOffset4 = value;
            if (index == 4) renderingParams.heightOffset5 = value;
            if (index == 5) renderingParams.heightOffset6 = value;
            if (index == 6) renderingParams.heightOffset7 = value;
            if (index == 7) renderingParams.heightOffset8 = value;
        }

        public static float GetHeightOffsetUI(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.heightOffset1;
            if (index == 1) return renderingParams.heightOffset2;
            if (index == 2) return renderingParams.heightOffset3;
            if (index == 3) return renderingParams.heightOffset4;
            if (index == 4) return renderingParams.heightOffset5;
            if (index == 5) return renderingParams.heightOffset6;
            if (index == 6) return renderingParams.heightOffset7;
            if (index == 7) return renderingParams.heightOffset8;

            return 0;
        }

        public static void SetDisplacementUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.displacement1 = value;
            if (index == 1) renderingParams.displacement2 = value;
            if (index == 2) renderingParams.displacement3 = value;
            if (index == 3) renderingParams.displacement4 = value;
            if (index == 4) renderingParams.displacement5 = value;
            if (index == 5) renderingParams.displacement6 = value;
            if (index == 6) renderingParams.displacement7 = value;
            if (index == 7) renderingParams.displacement8 = value;
        }

        public static float GetLayerNormalScaleUI(ref TerrainRenderingParams renderingParams, int index)
        {
            return GetLayerNormalScale(index);

            //if (index == 0) return renderingParams.layerNormalScale1;
            //if (index == 1) return renderingParams.layerNormalScale2;
            //if (index == 2) return renderingParams.layerNormalScale3;
            //if (index == 3) return renderingParams.layerNormalScale4;
            //if (index == 4) return renderingParams.layerNormalScale5;
            //if (index == 5) return renderingParams.layerNormalScale6;
            //if (index == 6) return renderingParams.layerNormalScale7;
            //if (index == 7) return renderingParams.layerNormalScale8;
            //
            //return 0;
        }

        public static void SetLayerNormalScaleUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            SetLayerNormalScale(index, value);

            //if (index == 0) renderingParams.layerNormalScale1 = value;
            //if (index == 1) renderingParams.layerNormalScale2 = value;
            //if (index == 2) renderingParams.layerNormalScale3 = value;
            //if (index == 3) renderingParams.layerNormalScale4 = value;
            //if (index == 4) renderingParams.layerNormalScale5 = value;
            //if (index == 5) renderingParams.layerNormalScale6 = value;
            //if (index == 6) renderingParams.layerNormalScale7 = value;
            //if (index == 7) renderingParams.layerNormalScale8 = value;
        }

        public static float GetLayerMetallicUI(ref TerrainRenderingParams renderingParams, int index)
        {
            return GetLayerMetallic(index);

            //if (index == 0) return renderingParams.layerMetallic1 = GetLayerMetallic(index);
            //if (index == 1) return renderingParams.layerMetallic2 = GetLayerMetallic(index);
            //if (index == 2) return renderingParams.layerMetallic3 = GetLayerMetallic(index);
            //if (index == 3) return renderingParams.layerMetallic4 = GetLayerMetallic(index);
            //if (index == 4) return renderingParams.layerMetallic5 = GetLayerMetallic(index);
            //if (index == 5) return renderingParams.layerMetallic6 = GetLayerMetallic(index);
            //if (index == 6) return renderingParams.layerMetallic7 = GetLayerMetallic(index);
            //if (index == 7) return renderingParams.layerMetallic8 = GetLayerMetallic(index);
            //
            //return 0;
        }

        public static void SetLayerMetallicUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            //if (index == 0) renderingParams.layerMetallic1 = value;
            //if (index == 1) renderingParams.layerMetallic2 = value;
            //if (index == 2) renderingParams.layerMetallic3 = value;
            //if (index == 3) renderingParams.layerMetallic4 = value;
            //if (index == 4) renderingParams.layerMetallic5 = value;
            //if (index == 5) renderingParams.layerMetallic6 = value;
            //if (index == 6) renderingParams.layerMetallic7 = value;
            //if (index == 7) renderingParams.layerMetallic8 = value;

            SetLayerMetallic(index, value);
        }

        public static float GetLayerSmoothnessUI(ref TerrainRenderingParams renderingParams, int index)
        {
            return GetLayerSmoothness(index);

            //if (index == 0) return renderingParams.layerSmoothness1 = GetLayerSmoothness(index);
            //if (index == 1) return renderingParams.layerSmoothness2 = GetLayerSmoothness(index);
            //if (index == 2) return renderingParams.layerSmoothness3 = GetLayerSmoothness(index);
            //if (index == 3) return renderingParams.layerSmoothness4 = GetLayerSmoothness(index);
            //if (index == 4) return renderingParams.layerSmoothness5 = GetLayerSmoothness(index);
            //if (index == 5) return renderingParams.layerSmoothness6 = GetLayerSmoothness(index);
            //if (index == 6) return renderingParams.layerSmoothness7 = GetLayerSmoothness(index);
            //if (index == 7) return renderingParams.layerSmoothness8 = GetLayerSmoothness(index);
            //
            //return 0;
        }

        public static void SetLayerSmoothnessUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            SetLayerSmoothness(index, value);

            //if (index == 0) renderingParams.layerSmoothness1 = value;
            //if (index == 1) renderingParams.layerSmoothness2 = value;
            //if (index == 2) renderingParams.layerSmoothness3 = value;
            //if (index == 3) renderingParams.layerSmoothness4 = value;
            //if (index == 4) renderingParams.layerSmoothness5 = value;
            //if (index == 5) renderingParams.layerSmoothness6 = value;
            //if (index == 6) renderingParams.layerSmoothness7 = value;
            //if (index == 7) renderingParams.layerSmoothness8 = value;
        }

        public static float GetLayerAOUI(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.layerAO1;
            if (index == 1) return renderingParams.layerAO2;
            if (index == 2) return renderingParams.layerAO3;
            if (index == 3) return renderingParams.layerAO4;
            if (index == 4) return renderingParams.layerAO5;
            if (index == 5) return renderingParams.layerAO6;
            if (index == 6) return renderingParams.layerAO7;
            if (index == 7) return renderingParams.layerAO8;
            
            return 0;
        }

        public static void SetLayerAOUI(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.layerAO1 = value;
            if (index == 1) renderingParams.layerAO2 = value;
            if (index == 2) renderingParams.layerAO3 = value;
            if (index == 3) renderingParams.layerAO4 = value;
            if (index == 4) renderingParams.layerAO5 = value;
            if (index == 5) renderingParams.layerAO6 = value;
            if (index == 6) renderingParams.layerAO7 = value;
            if (index == 7) renderingParams.layerAO8 = value;
        }

        public static Vector4 GetLayerTilingUI(ref TerrainRenderingParams renderingParams, int index)
        {
            return GetLayerTiling(index);

            //if (index == 0) return renderingParams.layerTiling1 = GetLayerTiling(index);
            //if (index == 1) return renderingParams.layerTiling2 = GetLayerTiling(index);
            //if (index == 2) return renderingParams.layerTiling3 = GetLayerTiling(index);
            //if (index == 3) return renderingParams.layerTiling4 = GetLayerTiling(index);
            //if (index == 4) return renderingParams.layerTiling5 = GetLayerTiling(index);
            //if (index == 5) return renderingParams.layerTiling6 = GetLayerTiling(index);
            //if (index == 6) return renderingParams.layerTiling7 = GetLayerTiling(index);
            //if (index == 7) return renderingParams.layerTiling8 = GetLayerTiling(index);
            //
            //return new Vector4(4, 4, 0, 0);
        }

        public static void SetLayerTilingUI(ref TerrainRenderingParams renderingParams, int index, Vector4 value)
        {
            SetLayerTiling(index, value);

            //if (index == 0) renderingParams.layerTiling1 = value;
            //if (index == 1) renderingParams.layerTiling2 = value;
            //if (index == 2) renderingParams.layerTiling3 = value;
            //if (index == 3) renderingParams.layerTiling4 = value;
            //if (index == 4) renderingParams.layerTiling5 = value;
            //if (index == 5) renderingParams.layerTiling6 = value;
            //if (index == 6) renderingParams.layerTiling7 = value;
            //if (index == 7) renderingParams.layerTiling8 = value;
        }

        public static bool GetLayerProceduralTexturingUI(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.proceduralTexturing1;
            if (index == 1) return renderingParams.proceduralTexturing2;
            if (index == 2) return renderingParams.proceduralTexturing3;
            if (index == 3) return renderingParams.proceduralTexturing4;
            if (index == 4) return renderingParams.proceduralTexturing5;
            if (index == 5) return renderingParams.proceduralTexturing6;
            if (index == 6) return renderingParams.proceduralTexturing7;
            if (index == 7) return renderingParams.proceduralTexturing8;

            return false;
        }

        public static void SetLayerProceduralTexturingUI(ref TerrainRenderingParams renderingParams, int index, bool value)
        {
            if (index == 0) renderingParams.proceduralTexturing1 = value;
            if (index == 1) renderingParams.proceduralTexturing2 = value;
            if (index == 2) renderingParams.proceduralTexturing3 = value;
            if (index == 3) renderingParams.proceduralTexturing4 = value;
            if (index == 4) renderingParams.proceduralTexturing5 = value;
            if (index == 5) renderingParams.proceduralTexturing6 = value;
            if (index == 6) renderingParams.proceduralTexturing7 = value;
            if (index == 7) renderingParams.proceduralTexturing8 = value;
        }

        public static bool GetLayerGradientTexturingUI(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.gradientTexturing1;
            if (index == 1) return renderingParams.gradientTexturing2;
            if (index == 2) return renderingParams.gradientTexturing3;
            if (index == 3) return renderingParams.gradientTexturing4;
            if (index == 4) return renderingParams.gradientTexturing5;
            if (index == 5) return renderingParams.gradientTexturing6;
            if (index == 6) return renderingParams.gradientTexturing7;
            if (index == 7) return renderingParams.gradientTexturing8;

            return false;
        }

        public static void SetLayerGradientTexturingUI(ref TerrainRenderingParams renderingParams, int index, bool value)
        {
            if (index == 0) renderingParams.gradientTexturing1 = value;
            if (index == 1) renderingParams.gradientTexturing2 = value;
            if (index == 2) renderingParams.gradientTexturing3 = value;
            if (index == 3) renderingParams.gradientTexturing4 = value;
            if (index == 4) renderingParams.gradientTexturing5 = value;
            if (index == 5) renderingParams.gradientTexturing6 = value;
            if (index == 6) renderingParams.gradientTexturing7 = value;
            if (index == 7) renderingParams.gradientTexturing8 = value;
        }

        public static float GetLayerGradientTiling(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.gradientTiling1;
            if (index == 1) return renderingParams.gradientTiling2;
            if (index == 2) return renderingParams.gradientTiling3;
            if (index == 3) return renderingParams.gradientTiling4;
            if (index == 4) return renderingParams.gradientTiling5;
            if (index == 5) return renderingParams.gradientTiling6;
            if (index == 6) return renderingParams.gradientTiling7;
            if (index == 7) return renderingParams.gradientTiling8;

            return 1;
        }

        public static void SetLayerGradientTiling(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.gradientTiling1 = value;
            if (index == 1) renderingParams.gradientTiling2 = value;
            if (index == 2) renderingParams.gradientTiling3 = value;
            if (index == 3) renderingParams.gradientTiling4 = value;
            if (index == 4) renderingParams.gradientTiling5 = value;
            if (index == 5) renderingParams.gradientTiling6 = value;
            if (index == 6) renderingParams.gradientTiling7 = value;
            if (index == 7) renderingParams.gradientTiling8 = value;
        }

        public static float GetLayerColorDamping(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.colorDamping1;
            if (index == 1) return renderingParams.colorDamping2;
            if (index == 2) return renderingParams.colorDamping3;
            if (index == 3) return renderingParams.colorDamping4;
            if (index == 4) return renderingParams.colorDamping5;
            if (index == 5) return renderingParams.colorDamping6;
            if (index == 6) return renderingParams.colorDamping7;
            if (index == 7) return renderingParams.colorDamping8;

            return 0;
        }

        public static void SetLayerColorDamping(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.colorDamping1 = value;
            if (index == 1) renderingParams.colorDamping2 = value;
            if (index == 2) renderingParams.colorDamping3 = value;
            if (index == 3) renderingParams.colorDamping4 = value;
            if (index == 4) renderingParams.colorDamping5 = value;
            if (index == 5) renderingParams.colorDamping6 = value;
            if (index == 6) renderingParams.colorDamping7 = value;
            if (index == 7) renderingParams.colorDamping8 = value;
        }

        public static float GetLayerSlopeInfluence(ref TerrainRenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.slopeInfluence1;
            if (index == 1) return renderingParams.slopeInfluence2;
            if (index == 2) return renderingParams.slopeInfluence3;
            if (index == 3) return renderingParams.slopeInfluence4;
            if (index == 4) return renderingParams.slopeInfluence5;
            if (index == 5) return renderingParams.slopeInfluence6;
            if (index == 6) return renderingParams.slopeInfluence7;
            if (index == 7) return renderingParams.slopeInfluence8;

            return 1;
        }

        public static void SetLayerSlopeInfluence(ref TerrainRenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.slopeInfluence1 = value;
            if (index == 1) renderingParams.slopeInfluence2 = value;
            if (index == 2) renderingParams.slopeInfluence3 = value;
            if (index == 3) renderingParams.slopeInfluence4 = value;
            if (index == 4) renderingParams.slopeInfluence5 = value;
            if (index == 5) renderingParams.slopeInfluence6 = value;
            if (index == 6) renderingParams.slopeInfluence7 = value;
            if (index == 7) renderingParams.slopeInfluence8 = value;
        }
    }
#endif
}
#endif
#endif

