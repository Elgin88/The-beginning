#ifndef TERRASTANDARD_CORE_CGINC_INCLUDED
#define TERRASTANDARD_CORE_CGINC_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "TerraStandard_Input.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardBRDF.cginc"
#include "AutoLight.cginc"

#ifdef _PROCEDURALSNOW
    #include "TerraStandard_Snow.cginc" // Procedural Snow
#endif

#ifdef _WIND
	#include "TerraStandard_Wind.cginc" // Wind Simulation
#endif

#ifdef _FLATSHADING
	#include "TerraStandard_FlatShading.cginc" // Flat Shading
#endif

#ifdef _COLORMAP_BLENDING
	#include "TerraStandard_SatelliteImageBlending.cginc" // Colormap Blending
#endif

//#include "TerraStandard_Lighting.cginc"

sampler2D_float _CameraDepthTexture;

//-------------------------------------------------------------------------------------
// counterpart for NormalizePerPixelNormal
// skips normalization per-vertex and expects normalization to happen per-pixel
half3 NormalizePerVertexNormal (float3 n) // takes float to avoid overflow
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return normalize(n);
    #else
        return n; // will normalize per-pixel instead
    #endif
}

float3 NormalizePerPixelNormal (float3 n)
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return n;
    #else
        return normalize((float3)n); // takes float to avoid overflow
    #endif
}

//-------------------------------------------------------------------------------------
UnityLight MainLight ()
{
    UnityLight l;

    l.color = _LightColor0.rgb;
    l.dir = _WorldSpaceLightPos0.xyz;
    return l;
}

UnityLight AdditiveLight (half3 lightDir, half atten)
{
    UnityLight l;

    l.color = _LightColor0.rgb;
    l.dir = lightDir;
    #ifndef USING_DIRECTIONAL_LIGHT
        l.dir = NormalizePerPixelNormal(l.dir);
    #endif

    // shadow the light
    l.color *= atten;
    return l;
}

UnityLight DummyLight ()
{
    UnityLight l;
    l.color = 0;
    l.dir = half3 (0,1,0);
    return l;
}

UnityIndirect ZeroIndirect ()
{
    UnityIndirect ind;
    ind.diffuse = 0;
    ind.specular = 0;
    return ind;
}

//-------------------------------------------------------------------------------------
// Common fragment setup

// deprecated
half3 WorldNormal(half4 tan2world[3])
{
    return normalize(tan2world[2].xyz);
}

// deprecated
#ifdef _TANGENT_TO_WORLD
    half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
    {
        half3 t = tan2world[0].xyz;
        half3 b = tan2world[1].xyz;
        half3 n = tan2world[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        n = NormalizePerPixelNormal(n);

        // ortho-normalize Tangent
        t = normalize (t - n * dot(t, n));

        // recalculate Binormal
        half3 newB = cross(n, t);
        b = newB * sign (dot (newB, b));
    #endif

        return half3x3(t, b, n);
    }
#else
    half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
    {
        return half3x3(0,0,0,0,0,0,0,0,0);
    }
#endif

float3 PerPixelWorldNormal(float4 i_tex, float4 tangentToWorld[3])
{
#ifdef _NORMALMAP
    half3 tangent = tangentToWorld[0].xyz;
    half3 binormal = tangentToWorld[1].xyz;
    half3 normal = tangentToWorld[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        normal = NormalizePerPixelNormal(normal);

        // ortho-normalize Tangent
        tangent = normalize (tangent - normal * dot(tangent, normal));

        // recalculate Binormal
        half3 newB = cross(normal, tangent);
        binormal = newB * sign (dot (newB, binormal));
    #endif

    half3 normalTangent = NormalInTangentSpace(i_tex);
    float3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
#else
    float3 normalWorld = normalize(tangentToWorld[2].xyz);
#endif
    return normalWorld;
}

#ifdef _PARALLAXMAP
    #define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w))
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) NormalizePerPixelNormal(i.viewDirForParallax.xyz)
#else
    #define IN_VIEWDIR4PARALLAX(i) half3(0,0,0)
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) half3(0,0,0)
#endif

#if UNITY_REQUIRE_FRAG_WORLDPOS
    #if UNITY_PACK_WORLDPOS_WITH_TANGENT
        #define IN_WORLDPOS(i) half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w)
    #else
        #define IN_WORLDPOS(i) i.posWorld
    #endif
    #define IN_WORLDPOS_FWDADD(i) i.posWorld
#else
    #define IN_WORLDPOS(i) half3(0,0,0)
    #define IN_WORLDPOS_FWDADD(i) half3(0,0,0)
#endif

#define IN_LIGHTDIR_FWDADD(i) half3(i.tangentToWorldAndLightDir[0].w, i.tangentToWorldAndLightDir[1].w, i.tangentToWorldAndLightDir[2].w)

#define FRAGMENT_SETUP(x) FragmentCommonData x = \
    FragmentSetup(i.tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i));

#define FRAGMENT_SETUP_FWDADD(x) FragmentCommonData x = \
    FragmentSetup(i.tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX_FWDADD(i), i.tangentToWorldAndLightDir, IN_WORLDPOS_FWDADD(i));

struct FragmentCommonData
{
    half3 diffColor, specColor;
    // Note: smoothness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
    // Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
    half oneMinusReflectivity, smoothness;
    float3 normalWorld;
    float3 eyeVec;
    half alpha;
    float3 posWorld;

#if UNITY_STANDARD_SIMPLE
    half3 reflUVW;
#endif

#if UNITY_STANDARD_SIMPLE
    half3 tangentSpaceNormal;
#endif
};

#ifndef UNITY_SETUP_BRDF_INPUT
    #define UNITY_SETUP_BRDF_INPUT SpecularSetup
#endif

inline FragmentCommonData SpecularSetup (float4 i_tex)
{
    half4 specGloss = SpecularGloss(i_tex.xy);
    half3 specColor = specGloss.rgb;
    half smoothness = specGloss.a;

    half oneMinusReflectivity;
    half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular (Albedo(i_tex), specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    return o;
}

inline FragmentCommonData RoughnessSetup(float4 i_tex)
{
    half2 metallicGloss = MetallicRough(i_tex.xy);
    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    half3 diffColor = DiffuseAndSpecularFromMetallic(Albedo(i_tex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    return o;
}

inline FragmentCommonData MetallicSetup (float4 i_tex)
{
    half2 metallicGloss = MetallicGloss(i_tex.xy);
    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    half3 diffColor = DiffuseAndSpecularFromMetallic (Albedo(i_tex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    return o;
}

float hash(float n)
{
    return frac(sin(n) * 43758.5453);
}

// The noise function returns a value in the range -1.0f -> 1.0f
float SimplexNoise(float3 x)
{
    float3 p = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
    float n = p.x + p.y * 57.0 + 113.0 * p.z;

    return lerp
    (
        lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
        lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
        lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
        lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z
    );
}

float random (float2 uv)
{
    return frac(sin(dot(uv,float2(12.9898,78.233))) * 43758.5453123);
}

// parallax transformed texcoord is used to sample occlusion
inline FragmentCommonData FragmentSetup (inout float4 i_tex, float3 i_eyeVec, half3 i_viewDirForParallax, float4 tangentToWorld[3], float3 i_posWorld)
{
    i_tex = Parallax(i_tex, i_viewDirForParallax);

    half alpha = Alpha(i_tex.xy);

    //float4 screenPos = ComputeScreenPos(i_tex);
    //float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, screenPos);
    //float depth = LinearEyeDepth(depthSample);
    //float _DepthFactor = 0.5;
    //float foamLine = saturate(_DepthFactor * (depth - screenPos.w));
    //alpha = foamLine;


    #if defined(_ALPHATEST_ON)
        //// Colormap Blending
        //#ifdef _COLORMAP_BLENDING
        //    //float2 scale = float2
        //    //(
        //    //    length(unity_ObjectToWorld._m00_m10_m20),
        //    //    length(unity_ObjectToWorld._m02_m12_m22)
        //    //);
        //    //
        //    //float2 uv = (i_posWorld.xz - unity_WorldToObject._m03_m23) * scale;
        //    //float2 worldUV = (TRANSFORM_TEX(i_tex.xy, _MainTex) / _WorldSize.x) + 0.5;
        //    //
        //    //
        //    //alpha = Alpha(i_tex.xy);
        //
        //    //
        //    //
        //    //
        //    //
        //    //float fadeout = saturate(pow(i.tex.y + 0.5, _ColormapInfluence));
        //    //float4 colormap = tex2D(_Colormap, worldUV);
        //    ////lerp(clip (alpha - (_Cutoff * 0.1)), clip (alpha - _Cutoff), colormap.r);
        //    //float _NoiseTiling1 = 100;
        //    //float noise = SimplexNoise(20 * float3(worldUV, 0));
        //    //float noise = hash(i_tex.xy);
        //    
        //    //float noise = random(worldUV.xy);
        //    //clip (alpha - (_Cutoff * i_viewDirForParallax.xy));
        //    //clip (alpha - (_Cutoff * noise));
        //    clip (alpha - _Cutoff);
        //#else
            clip (alpha - _Cutoff);


            //// Geom-blending
            //// apply depth texture
            //
            //float4 screenPos = ComputeScreenPos(i_tex);
            //
            //float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, screenPos);
            //float depth = LinearEyeDepth(depthSample).r;
            //
            //float _DepthFactor = 1;
            //
            //// create foamline
            //float foamLine = saturate(_DepthFactor * (depth - screenPos.w));
            ////foamLine.y = 1 - foamLine.y;
            //
            //alpha *= foamLine;
            //clip (alpha - _Cutoff);

        //#endif
    #endif

    FragmentCommonData o = UNITY_SETUP_BRDF_INPUT (i_tex);
    o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
    o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
    o.posWorld = i_posWorld;

    // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);

    //float4 screenPos = ComputeScreenPos(i_tex);
    //float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, screenPos);
    //float depth = LinearEyeDepth(depthSample).r;
    //float _DepthFactor = 0;
    //float foamLine = saturate(_DepthFactor * (depth - screenPos.w));
    //o.alpha *= foamLine;

    return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = s.posWorld;
    d.worldViewDir = -s.eyeVec;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
      d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
      d.boxMax[0] = unity_SpecCube0_BoxMax;
      d.probePosition[0] = unity_SpecCube0_ProbePosition;
      d.boxMax[1] = unity_SpecCube1_BoxMax;
      d.boxMin[1] = unity_SpecCube1_BoxMin;
      d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    if(reflections)
    {
        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
        // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif

        return UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
    }
    else
    {
        return UnityGlobalIllumination (d, occlusion, s.normalWorld);
    }
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
}


//-------------------------------------------------------------------------------------
half4 OutputForward (half4 output, half alphaFromSurface)
{
    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
        output.a = alphaFromSurface;
    #else
        UNITY_OPAQUE_ALPHA(output.a);
    #endif
    return output;
}

inline half4 VertexGIForward(VertexInput v, float3 posWorld, half3 normalWorld)
{
    half4 ambientOrLightmapUV = 0;
    // Static lightmaps
    #ifdef LIGHTMAP_ON
        ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        ambientOrLightmapUV.zw = 0;
    // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
    #elif UNITY_SHOULD_SAMPLE_SH
        #ifdef VERTEXLIGHT_ON
            // Approximated illumination from non-important point lights
            ambientOrLightmapUV.rgb = Shade4PointLights (
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, posWorld, normalWorld);
        #endif

        ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);
    #endif

    #ifdef DYNAMICLIGHTMAP_ON
        ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    return ambientOrLightmapUV;
}

// ------------------------------------------------------------------
//  Base forward pass (directional light, emission, lightmaps, ...)

struct VertexOutputForwardBase
{
    UNITY_POSITION(pos);
    float4 tex                            : TEXCOORD0;
    float4 eyeVec                         : TEXCOORD1;    // eyeVec.xyz | fogCoord
    float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
    half4 ambientOrLightmapUV             : TEXCOORD5;    // SH or Lightmap UV
    UNITY_LIGHTING_COORDS(6,7)

    // next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
    float3 posWorld                     : TEXCOORD8;
#endif

    // UV3 Texturing
    float2 uv3_texcoord3 : TEXCOORD9;

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    float2 worldUV : TEXCOORD10;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputForwardBase vertForwardBase (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputForwardBase o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    // Flat Shading
#ifdef _FLATSHADING
    float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
    FlatShading(v.vertex.xyz, v.normal, worldNormal, TexCoords(v).xy);
#endif

	// Wind Simulation
#ifdef _WIND
    WindSimulation(v.vertex, v.normal, v.uv0.y);
#endif

    // UV3 Texturing
#ifdef _DETALUSEUV3_ON
    o.uv3_texcoord3 = TexCoords(v).zw;
#endif

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    #if UNITY_REQUIRE_FRAG_WORLDPOS
        #if UNITY_PACK_WORLDPOS_WITH_TANGENT
            o.tangentToWorldAndPackedData[0].w = posWorld.x;
            o.tangentToWorldAndPackedData[1].w = posWorld.y;
            o.tangentToWorldAndPackedData[2].w = posWorld.z;
        #else
            o.posWorld = posWorld.xyz;
        #endif
    #endif
    o.pos = UnityObjectToClipPos(v.vertex);

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    float2 scale = float2
    (
        length(unity_ObjectToWorld._m00_m10_m20),
        length(unity_ObjectToWorld._m02_m12_m22)
    );
 
    float2 uv = (v.vertex.xz - unity_WorldToObject._m03_m23) * scale;
    o.worldUV = (TRANSFORM_TEX(uv, _MainTex) / _WorldSize.x) + 0.5;
#endif

    o.tex = TexCoords(v);
    o.eyeVec.xyz = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = normalWorld;
    #endif

    //We need this for shadow receving
    UNITY_TRANSFER_LIGHTING(o, v.uv1);

    o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);

    #ifdef _PARALLAXMAP
        TANGENT_SPACE_ROTATION;
        half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
        o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
        o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
    #endif

    UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,o.pos);
    return o;
}

half4 fragForwardBaseInternal (VertexOutputForwardBase i)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    FRAGMENT_SETUP(s)

    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    UnityLight mainLight = MainLight ();
    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

    half occlusion = Occlusion(i.tex.xy);
    UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    SatelliteImageBlending(s.diffColor, i.tex, i.worldUV, _Colormap);
#endif

    // UV3 Texturing
#ifdef _DETALUSEUV3_ON
    float2 uv0_DetailAlbedoMap = i.tex * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    float2 uv2_DetailNormalMap = i.uv3_texcoord3 * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    float2 staticSwitch280 = uv2_DetailNormalMap;

    float2 uv_DetailMask = i.tex * _DetailMask_ST.xy + _DetailMask_ST.zw;
    float4 tex2DNode25 = tex2D(_DetailMask, uv_DetailMask);

    float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, staticSwitch280), _DetailNormalMapScale);
    float3 lerpResult19 = lerp(s.normalWorld, detailNormal, tex2DNode25.a);
    s.normalWorld = lerpResult19;

    float3 detailColor = tex2D(_DetailAlbedoMap, staticSwitch280);
    float3 lerpResult16 = lerp(s.diffColor, detailColor, tex2DNode25.a);
    s.diffColor = (lerpResult16 * _Color).rgb;
#endif

    // Flat Shading
#ifdef _FLATSHADING
    if (_FlatShadingState == 1) s.normalWorld = normalize(cross(ddy(s.posWorld), ddx(s.posWorld)));
#endif

	// Procedural Snow
#ifdef _PROCEDURALSNOW
    ProceduralSnow(s.diffColor, s.smoothness, i.tex.xy, s.posWorld.y, s.normalWorld);
#endif

    half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
    c.rgb += Emission(i.tex.xy);

    UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
    UNITY_APPLY_FOG(_unity_fogCoord, c.rgb);
    return OutputForward (c, s.alpha);
}

half4 fragForwardBase (VertexOutputForwardBase i) : SV_Target   // backward compatibility (this used to be the fragment entry function)
{
    return fragForwardBaseInternal(i);
}

// ------------------------------------------------------------------
//  Additive forward pass (one light per pass)

struct VertexOutputForwardAdd
{
    UNITY_POSITION(pos);
    float4 tex                          : TEXCOORD0;
    float4 eyeVec                       : TEXCOORD1;    // eyeVec.xyz | fogCoord
    float4 tangentToWorldAndLightDir[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:lightDir]
    float3 posWorld                     : TEXCOORD5;
    UNITY_LIGHTING_COORDS(6, 7)

    // next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if defined(_PARALLAXMAP)
    half3 viewDirForParallax            : TEXCOORD8;
#endif

    // UV3 Texturing
    float2 uv3_texcoord3 : TEXCOORD9;

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    float2 worldUV : TEXCOORD10;
#endif

    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputForwardAdd vertForwardAdd (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputForwardAdd o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    // Flat Shading
#ifdef _FLATSHADING
    float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
    FlatShading(v.vertex.xyz, v.normal, worldNormal, TexCoords(v).xy);
#endif

	// Wind Simulation
#ifdef _WIND
    WindSimulation(v.vertex, v.normal, v.uv0.y);
#endif

    // UV3 Texturing
#ifdef _DETALUSEUV3_ON
    o.uv3_texcoord3 = TexCoords(v).zw;
#endif

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.pos = UnityObjectToClipPos(v.vertex);

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    float2 scale = float2
    (
        length(unity_ObjectToWorld._m00_m10_m20),
        length(unity_ObjectToWorld._m02_m12_m22)
    );
 
    float2 uv = (v.vertex.xz - unity_WorldToObject._m03_m23) * scale;
    o.worldUV = (TRANSFORM_TEX(uv, _MainTex) / _WorldSize.x) + 0.5;
#endif

    o.tex = TexCoords(v);
    o.eyeVec.xyz = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    o.posWorld = posWorld.xyz;
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndLightDir[0].xyz = 0;
        o.tangentToWorldAndLightDir[1].xyz = 0;
        o.tangentToWorldAndLightDir[2].xyz = normalWorld;
    #endif
    //We need this for shadow receiving and lighting
    UNITY_TRANSFER_LIGHTING(o, v.uv1);

    float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;
    #ifndef USING_DIRECTIONAL_LIGHT
        lightDir = NormalizePerVertexNormal(lightDir);
    #endif
    o.tangentToWorldAndLightDir[0].w = lightDir.x;
    o.tangentToWorldAndLightDir[1].w = lightDir.y;
    o.tangentToWorldAndLightDir[2].w = lightDir.z;

    #ifdef _PARALLAXMAP
        TANGENT_SPACE_ROTATION;
        o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
    #endif

    UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o, o.pos);
    return o;
}

half4 fragForwardAddInternal (VertexOutputForwardAdd i)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    FRAGMENT_SETUP_FWDADD(s)

    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld)
    UnityLight light = AdditiveLight (IN_LIGHTDIR_FWDADD(i), atten);
    UnityIndirect noIndirect = ZeroIndirect ();
    
    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    SatelliteImageBlending(s.diffColor, i.tex, i.worldUV, _Colormap);
#endif

    // UV3 Texturing
#ifdef _DETALUSEUV3_ON
    float2 uv0_DetailAlbedoMap = i.tex * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    float2 uv2_DetailNormalMap = i.uv3_texcoord3 * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    float2 staticSwitch280 = uv2_DetailNormalMap;

    float2 uv_DetailMask = i.tex * _DetailMask_ST.xy + _DetailMask_ST.zw;
    float4 tex2DNode25 = tex2D(_DetailMask, uv_DetailMask);

    float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, staticSwitch280), _DetailNormalMapScale);
    float3 lerpResult19 = lerp(s.normalWorld, detailNormal, tex2DNode25.a);
    s.normalWorld = lerpResult19;

    float3 detailColor = tex2D(_DetailAlbedoMap, staticSwitch280);
    float3 lerpResult16 = lerp(s.diffColor, detailColor, tex2DNode25.a);
    s.diffColor = (lerpResult16 * _Color).rgb;
#endif

    // Flat Shading
#ifdef _FLATSHADING
    if (_FlatShadingState == 1) s.normalWorld = normalize(cross(ddy(s.posWorld), ddx(s.posWorld)));
#endif

	// Procedural Snow
#ifdef _PROCEDURALSNOW
    ProceduralSnow(s.diffColor, s.smoothness, i.tex.xy, s.posWorld.y, s.normalWorld);
#endif

    half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, light, noIndirect);

    UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
    UNITY_APPLY_FOG_COLOR(_unity_fogCoord, c.rgb, half4(0,0,0,0)); // fog towards black in additive pass
    return OutputForward (c, s.alpha);
}

half4 fragForwardAdd (VertexOutputForwardAdd i) : SV_Target     // backward compatibility (this used to be the fragment entry function)
{
    return fragForwardAddInternal(i);
}

// ------------------------------------------------------------------
//  Deferred pass

struct VertexOutputDeferred
{
    UNITY_POSITION(pos);
    float4 tex                            : TEXCOORD0;
    float3 eyeVec                         : TEXCOORD1;
    float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
    half4 ambientOrLightmapUV             : TEXCOORD5;    // SH or Lightmap UVs
    #if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
        float3 posWorld                     : TEXCOORD6;
    #endif
    
    // UV3 Texturing
    float2 uv3_texcoord3 : TEXCOORD7;

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    float2 worldUV : TEXCOORD8;
#endif
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputDeferred vertDeferred (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputDeferred o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    // Flat Shading
#ifdef _FLATSHADING
    float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
    FlatShading(v.vertex.xyz, v.normal, worldNormal, TexCoords(v).xy);
#endif

	// Wind Simulation
#ifdef _WIND
    WindSimulation(v.vertex, v.normal, v.uv0.y);
#endif

    // UV3 Texturing
#ifdef _DETALUSEUV3_ON
    o.uv3_texcoord3 = TexCoords(v).zw;
#endif

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    #if UNITY_REQUIRE_FRAG_WORLDPOS
        #if UNITY_PACK_WORLDPOS_WITH_TANGENT
            o.tangentToWorldAndPackedData[0].w = posWorld.x;
            o.tangentToWorldAndPackedData[1].w = posWorld.y;
            o.tangentToWorldAndPackedData[2].w = posWorld.z;
        #else
            o.posWorld = posWorld.xyz;
        #endif
    #endif
    o.pos = UnityObjectToClipPos(v.vertex);

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    float2 scale = float2
    (
        length(unity_ObjectToWorld._m00_m10_m20),
        length(unity_ObjectToWorld._m02_m12_m22)
    );
 
    float2 uv = (v.vertex.xz - unity_WorldToObject._m03_m23) * scale;
    o.worldUV = (TRANSFORM_TEX(uv, _MainTex) / _WorldSize.x) + 0.5;
#endif

    o.tex = TexCoords(v);
    o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = normalWorld;
    #endif

    o.ambientOrLightmapUV = 0;
    #ifdef LIGHTMAP_ON
        o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    #elif UNITY_SHOULD_SAMPLE_SH
        o.ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, o.ambientOrLightmapUV.rgb);
    #endif
    #ifdef DYNAMICLIGHTMAP_ON
        o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    #ifdef _PARALLAXMAP
        TANGENT_SPACE_ROTATION;
        half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
        o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
        o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
    #endif

    return o;
}

void fragDeferred (
    VertexOutputDeferred i,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    ,out half4 outShadowMask : SV_Target4       // RT4: shadowmask (rgba)
#endif
)
{
    #if (SHADER_TARGET < 30)
        outGBuffer0 = 1;
        outGBuffer1 = 1;
        outGBuffer2 = 0;
        outEmission = 0;
        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
            outShadowMask = 1;
        #endif
        return;
    #endif

    //FRAGMENT_SETUP(s)
    //UNITY_SETUP_INSTANCE_ID(i);
    //float4 screenPos = ComputeScreenPos(i.tex);
    //float2 vpos = screenPos.xy / screenPos.w * _ScreenParams.xy;
    //UNITY_APPLY_DITHER_CROSSFADE(vpos);

    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);
    FRAGMENT_SETUP(s)
    UNITY_SETUP_INSTANCE_ID(i);

    // no analytic lights in this pass
    UnityLight dummyLight = DummyLight ();
    half atten = 1;

    // only GI
    half occlusion = Occlusion(i.tex.xy);
#if UNITY_ENABLE_REFLECTION_BUFFERS
    bool sampleReflectionsInDeferred = false;
#else
    bool sampleReflectionsInDeferred = true;
#endif

    UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

    // Colormap Blending
#ifdef _COLORMAP_BLENDING
    SatelliteImageBlending(s.diffColor, i.tex, i.worldUV, _Colormap);
#endif

    // UV3 Texturing
#ifdef _DETALUSEUV3_ON
    float2 uv0_DetailAlbedoMap = i.tex * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    float2 uv2_DetailNormalMap = i.uv3_texcoord3 * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    float2 staticSwitch280 = uv2_DetailNormalMap;

    float2 uv_DetailMask = i.tex * _DetailMask_ST.xy + _DetailMask_ST.zw;
    float4 tex2DNode25 = tex2D(_DetailMask, uv_DetailMask);

    float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, staticSwitch280), _DetailNormalMapScale);
    float3 lerpResult19 = lerp(s.normalWorld, detailNormal, tex2DNode25.a);
    s.normalWorld = lerpResult19;

    float3 detailColor = tex2D(_DetailAlbedoMap, staticSwitch280);
    float3 lerpResult16 = lerp(s.diffColor, detailColor, tex2DNode25.a);
    s.diffColor = (lerpResult16 * _Color).rgb;
#endif

    // Flat Shading
#ifdef _FLATSHADING
    if (_FlatShadingState == 1) s.normalWorld = normalize(cross(ddy(s.posWorld), ddx(s.posWorld)));
#endif

	// Procedural Snow
#ifdef _PROCEDURALSNOW
    ProceduralSnow(s.diffColor, s.smoothness, i.tex.xy, s.posWorld.y, s.normalWorld);
#endif



    //// Geom-blending
    //// apply depth texture
    //
    //float4 screenPos = ComputeScreenPos(i.tex);
    //
    //float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, screenPos);
    //float depth = LinearEyeDepth(depthSample).r;
    //
    //float _DepthFactor = 1;
    //
    //// create foamline
    //float foamLine = saturate(_DepthFactor * (depth - screenPos.w));
    ////foamLine.y = 1 - foamLine.y;
    //
    //float4 col = float4(s.diffColor.rgb, foamLine);
    //s.alpha = foamLine;

    half3 emissiveColor = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;

    #ifdef _EMISSION
        emissiveColor += Emission (i.tex.xy);
    #endif

    #ifndef UNITY_HDR_ON
        emissiveColor.rgb = exp2(-emissiveColor.rgb);
    #endif

    UnityStandardData data;
    data.diffuseColor   = s.diffColor;
    data.occlusion      = occlusion;
    data.specularColor  = s.specColor;
    data.smoothness     = s.smoothness;
    data.normalWorld    = s.normalWorld;

    //float3 lightDir = _WorldSpaceLightPos0.xyz - s.posWorld.xyz * _WorldSpaceLightPos0.w;
    //data.diffuseColor = LightingRealWorld(s.normalWorld, s.smoothness, s.diffColor, lightDir, atten);
    
    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

    // Emissive lighting buffer
    outEmission = half4(emissiveColor, 1);

    // Baked direct lighting occlusion if any
    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i));
    #endif
}


//
// Old FragmentGI signature. Kept only for backward compatibility and will be removed soon
//

inline UnityGI FragmentGI(
    float3 posWorld,
    half occlusion, half4 i_ambientOrLightmapUV, half atten, half smoothness, half3 normalWorld, half3 eyeVec,
    UnityLight light,
    bool reflections)
{
    // we init only fields actually used
    FragmentCommonData s = (FragmentCommonData)0;
    s.smoothness = smoothness;
    s.normalWorld = normalWorld;
    s.eyeVec = eyeVec;
    s.posWorld = posWorld;
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, reflections);
}

inline UnityGI FragmentGI (
    float3 posWorld,
    half occlusion, half4 i_ambientOrLightmapUV, half atten, half smoothness, half3 normalWorld, half3 eyeVec,
    UnityLight light)
{
    return FragmentGI (posWorld, occlusion, i_ambientOrLightmapUV, atten, smoothness, normalWorld, eyeVec, light, true);
}

#endif // TERRASTANDARD_CORE_INCLUDED

