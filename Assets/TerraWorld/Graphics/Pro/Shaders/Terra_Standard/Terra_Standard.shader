﻿// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "TerraUnity/Standard"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        // Double Sided Rendering
		[ToggleUI] _DoubleSidedEnable("Double sided enable", Float) = 0.0
        [Enum(Flip, 0, Mirror, 1, None, 2)] _DoubleSidedNormalMode("Double sided normal mode", Float) = 1
        [HideInInspector] _DoubleSidedConstants("_DoubleSidedConstants", Vector) = (1, 1, -1, 0)
		[HideInInspector] _CullMode("__cullmode", Float) = 2.0
        [HideInInspector] _CullModeForward("__cullmodeForward", Float) = 2.0 // This mode is dedicated to Forward to correctly handle backface then front face rendering thin transparent

		// Procedural Snow
        [Toggle] _ProceduralLayer("Procedural Layer", Float) = 1.0
		[Toggle] _ProceduralSnow("Procedural Snow", Float) = 1.0
        _SnowState("Snow Switch", Float) = 0.0
		_SnowColor("Snow Color", color) = (1, 1, 1, 1)
        _SnowTexture("Snow Texture", 2D) = "white" {}
		_SnowTiling("Snow Tiling", Float) = 10.0
        _SnowDirection ("Snow Direction", Vector) = (0, 1, 0, 1)
        _SnowLevel ("Snow Level", Range(0, 1)) = 0.5
        _SnowSmoothness ("Snow Smoothness", Range(0, 1)) = 1.0
        _SnowStartHeight("Start Height", Float) = 5000.0
        _HeightFalloff ("Falloff",  Range (0, 10000) ) = 1000
		_SnowThickness ("Thickness", Float) = 1
		_SnowDamping ("Damping", Float) = 1
		
		// Wind Simulation
        [Space(10)]
		[Toggle] _Wind("Wind", Float) = 1.0
        //_WindState("Wind Switch", Float) = 0.0

        // Manual Type Settings
        [Toggle(_IS_LEAVES)]
        _IsLeaves("Is Leaves", Float) = 0.0
        [Toggle(_IS_BARK)]
        _IsBark("Is Bark", Float) = 0.0
        [Toggle(_IS_GRASS)]
        _IsGrass("Is Grass", Float) = 0.0

        [Space(10)]
        _BaseWindMultipliers("Wind Multipliers (XYZ)*", Vector) = (1, 1, 0.1, 0)
        [Space(3)]
        _TumbleStrength("Tumble Strength", Range(-1, 1)) = 0.05
        _TumbleFrequency("    Tumble Frequency", Range(0, 4)) = 0.1
        _TimeOffset("    Time Offset", Range(0, 2)) = 0.25
        [Space(3)]
        //[Toggle(_EMISSION)]
        _EnableLeafTurbulence("Enable Leaf Turbulence", Float) = 0.0
        _LeafTurbulence("    Leaf Turbulence", Range(0,4)) = 0.1
        _EdgeFlutterInfluence("    Edge Flutter Influence", Range(0,1)) = 0.25
        [Space(3)]
        [Toggle(GEOM_TYPE_LEAF)]
        _EnableAdvancedEdgeBending("Enable advanced Edge Flutter", Float) = 0.0
        [CTI_AdvancedEdgeFluttering]
        _AdvancedEdgeBending("    Strength (X) Frequency (Y) ", Vector) = (0.1, 1, 0, 0)
        [Space(5)]
        //[Toggle(_METALLICGLOSSMAP)]
        _LODTerrain("Use Wind from Script*", Float) = 0.0
        [Space(5)]
        //[Toggle(_NORMALMAP)]
        _AnimateNormal("Enable Normal Rotation", Float) = 0.0
        [Space(10)]

        [Header(Options for lowest LOD)]
        [Space(3)]
        [Toggle] _FadeOutWind("Fade out Wind", Float) = 0.0

        // UV3 Texturing
        [Toggle(_DETALUSEUV3_ON)] _DetalUseUV3("Detal Use UV3", Float) = 0

        //_TerrainLODWind("Leaves & Bark Wind Params", Vector) = (0, 0, 0, 0)
        //_WindSpeedMultiplier("Wind Speed Multiplier", Float) = 1.0
        _ShakeTime("Time", Float) = 0.85
        _ShakeWindspeed("Speed", Float) = 1.25
        _ShakeBending("Bending", Float) = 1.33

        // Flat Shading
        //TODO: Check if this toggle works or not!
		[Toggle] _FlatShading("Flat Shading", Float) = 0.0
        _FlatShadingState("Flat Shading Switch", Float) = 0.0
        _FlatShadingAngleMin("Min Slope", Float) = 0.0
        _FlatShadingAngleMax("Max Slope", Float) = 0.0
        _FlatShadingStrengthObject("Vertex Displacement", Float) = 0.0

        // Colormap Blending
        [Toggle(_COLORMAP_BLENDING)] _ColormapBlending("Colormap Blending", Float) = 0
        _Colormap("Colormap", 2D) = "white" {}
        _ColormapInfluence("Bottom To Top Influence", Range(0, 10)) = 5
        [HideInInspector] _WorldSize ("World Size", Vector) = (4000, 4000, 0, 0)
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
			Cull [_CullMode]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _PARALLAXMAP

            #pragma shader_feature GEOM_TYPE_LEAF
            #pragma shader_feature _IS_LEAVES
            #pragma shader_feature _IS_BARK
            #pragma shader_feature _IS_GRASS
            #pragma shader_feature _DETALUSEUV3_ON
            #pragma shader_feature _COLORMAP_BLENDING

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            #pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile __ _PROCEDURALSNOW
			#pragma multi_compile __ _WIND
            #pragma multi_compile __ _FLATSHADING

            #pragma vertex vertBase
            #pragma fragment fragBase
            //#include "UnityStandardCoreForward.cginc"
			#include "TerraStandard_Core_Forward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual
			Cull [_CullMode]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP
            
            #pragma shader_feature GEOM_TYPE_LEAF
            #pragma shader_feature _IS_LEAVES
            #pragma shader_feature _IS_BARK
            #pragma shader_feature _IS_GRASS
            #pragma shader_feature _DETALUSEUV3_ON
            #pragma shader_feature _COLORMAP_BLENDING

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            #pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile __ _PROCEDURALSNOW
			#pragma multi_compile __ _WIND
            #pragma multi_compile __ _FLATSHADING

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            //#include "UnityStandardCoreForward.cginc"
			#include "TerraStandard_Core_Forward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual
			Cull [_CullMode]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _PARALLAXMAP

            #pragma shader_feature GEOM_TYPE_LEAF
            #pragma shader_feature _IS_LEAVES
            #pragma shader_feature _IS_BARK
            #pragma shader_feature _IS_GRASS

            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma multi_compile __ _WIND
            #pragma multi_compile __ _FLATSHADING

			//#pragma vertex vs_tess
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            //#include "UnityStandardShadow.cginc"
			#include "TerraStandard_Shadow.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Deferred pass
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

			Cull [_CullMode]

            CGPROGRAM
            //#pragma surface surf RealWorld fullforwardshadows
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_vertex LOD_FADE_PERCENTAGE

            // AdvancedEdgeBending
            #pragma shader_feature GEOM_TYPE_LEAF
            #pragma shader_feature _IS_LEAVES
            #pragma shader_feature _IS_BARK
            #pragma shader_feature _IS_GRASS
            #pragma shader_feature _DETALUSEUV3_ON
            #pragma shader_feature _COLORMAP_BLENDING

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            #pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile __ _PROCEDURALSNOW
			#pragma multi_compile __ _WIND
            #pragma multi_compile __ _FLATSHADING

            //UNITY_INSTANCING_BUFFER_START(Props)
            //    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            //UNITY_INSTANCING_BUFFER_END(Props)

            #pragma vertex vertDeferred
            #pragma fragment fragDeferred

            //#include "UnityStandardCore.cginc"

            #include "TerraStandard_Input.cginc"
			#include "TerraStandard_Core.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
			Cull [_CullMode]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            // SM2.0: NOT SUPPORTED shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma vertex vertBase
            #pragma fragment fragBase
            //#include "UnityStandardCoreForward.cginc"
			#include "TerraStandard_Core_Forward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual
			Cull [_CullMode]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT
            
            #pragma shader_feature GEOM_TYPE_LEAF
            #pragma shader_feature _IS_LEAVES
            #pragma shader_feature _IS_BARK
            #pragma shader_feature _IS_GRASS
            #pragma shader_feature _DETALUSEUV3_ON
            #pragma shader_feature _COLORMAP_BLENDING

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            //#include "UnityStandardCoreForward.cginc"
			#include "TerraStandard_Core_Forward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual
			Cull [_CullMode]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            
            #pragma shader_feature GEOM_TYPE_LEAF
            #pragma shader_feature _IS_LEAVES
            #pragma shader_feature _IS_BARK
            #pragma shader_feature _IS_GRASS

            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma multi_compile __ _WIND
            #pragma multi_compile __ _FLATSHADING

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            //#include "UnityStandardShadow.cginc"
			#include "TerraStandard_Shadow.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    FallBack "VertexLit"
    CustomEditor "TerraStandard_GUI"
}

