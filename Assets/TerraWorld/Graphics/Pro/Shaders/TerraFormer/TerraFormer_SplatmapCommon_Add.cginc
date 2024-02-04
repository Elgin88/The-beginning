#ifndef TERRAFORMER_SPLATMAPCOMMON_ADD_CGINC_INCLUDED
#define TERRAFORMER_SPLATMAPCOMMON_ADD_CGINC_INCLUDED

float _Phong;
half _Metallic0;
half _Metallic1;
half _Metallic2;
half _Metallic3;
half _Smoothness0;
half _Smoothness1;
half _Smoothness2;
half _Smoothness3;
sampler2D _Control;
float4 _Control_TexelSize;
UNITY_DECLARE_TEX2D(_Splat0);
UNITY_DECLARE_TEX2D(_Splat1);
UNITY_DECLARE_TEX2D(_Splat2);
UNITY_DECLARE_TEX2D(_Splat3);
float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
sampler2D _Mask0, _Mask1, _Mask2, _Mask3;
UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap4);
UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap5);
UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap6);
UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap7);
float _TilingRemover5, _TilingRemover6, _TilingRemover7, _TilingRemover8;
float _NoiseTiling5, _NoiseTiling6, _NoiseTiling7, _NoiseTiling8;
fixed4 _LayerColor5, _LayerColor6, _LayerColor7, _LayerColor8;
fixed4 _LightingColor;
half _LayerAO5, _LayerAO6, _LayerAO7, _LayerAO8;

//sampler2D _Noise;
float _NoiseTiling;
//float _NoiseIntensity;

UNITY_DECLARE_TEX2D_NOSAMPLER(_WaterMask);
//float _WeightOpacity;
sampler2D _Control0;

//#ifdef _NORMALMAP
	sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
	float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
//#endif

struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float2 texcoord2 : TEXCOORD2;
	//float3 worldNormal : TEXCOORD3;
};

float4 tessEdge(appdata v0, appdata v1, appdata v2)
{
	#ifdef _TESSELLATION
		return UnityEdgeLengthBasedTessCull(v0.vertex, v1.vertex, v2.vertex, _EdgeLength, 16 * 1.5f);
		//return UnityDistanceBasedTess (v0.vertex, v1.vertex, v2.vertex, 30, 100, _EdgeLength * 1.28);
	#else
		return float4(1, 1, 1, 1);
	#endif
}

struct Input
{
	float2 uv_Control : TEXCOORD0;
	float4 color : COLOR;
	float3 worldNormal;
	float3 worldPos;

#ifndef TERRAIN_BASE_PASS
	UNITY_FOG_COORDS(0) // needed because finalcolor oppresses fog code generation.
#endif

	INTERNAL_DATA
};

void SplatmapVert(inout appdata v)
{
	//float textureSize = _TerrainSize / _SplatTiling1;
	//float textureSize = 15;
	//float2 xxx = TRANSFORM_TEX(v.texcoord, _Splat0);

	//float d = tex2Dlod(_Splat0, float4(v.texcoord.xy * _Splat0_ST.xy, 0, 0)).a * _Depth1;
	//float d = tex2Dlod(_Splat0, float4(v.texcoord.xy * textureSize, 0, 0)).a * _Depth1;
	//float d = splat_control.r * tex2Dlod(_Heightmap0, float4(v.texcoord.xy * _SplatTiling1, 0, 0)).r * _Depth1;

	//float fade1 = 1 - splat_control.r;

	float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;

    // Flat Shading
#ifdef _FLATSHADING
	FlatShadingVert(v.vertex.xyz, v.normal, worldNormal, v.texcoord.xy);
#endif

#ifdef _TESSELLATION
	fixed4 splat_control = tex2Dlod(_Control, float4(v.texcoord.xy, 0, 0));
	//fixed splatSum = dot(splat_control, fixed4(1, 1, 1, 1));

	//half t = dot(worldNormal, normalize(half3(0, 1, 0)));
	float height1, height2, height3, height4;
	float4 uv0 = float4(v.texcoord.xy * _Splat0_ST.xy, 0, 0);
	float4 uv1 = float4(v.texcoord.xy * _Splat1_ST.xy, 0, 0);
	float4 uv2 = float4(v.texcoord.xy * _Splat2_ST.xy, 0, 0);
	float4 uv3 = float4(v.texcoord.xy * _Splat3_ST.xy, 0, 0);

	//float noise1 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling5, 0, 0)).r;
	//float noise2 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling6, 0, 0)).r;
	//float noise3 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling7, 0, 0)).r;
	//float noise4 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling8, 0, 0)).r;

	//if (_TilingRemover5 > 0.0f) uv0 = TilingRemoverVert(_TilingRemover5, uv0, noise1);
	//if (_TilingRemover6 > 0.0f) uv1 = TilingRemoverVert(_TilingRemover6, uv1, noise2);
	//if (_TilingRemover7 > 0.0f) uv2 = TilingRemoverVert(_TilingRemover7, uv2, noise3);
	//if (_TilingRemover8 > 0.0f) uv3 = TilingRemoverVert(_TilingRemover8, uv3, noise4);

	if (_TilingRemover5 > 0.0f)
	{
		float noise = SimplexNoise(v.texcoord.xy, _NoiseTiling5 * 10);
		uv0 = TilingRemoverVert(_TilingRemover5, uv0, noise);
	}

	if (_TilingRemover6 > 0.0f)
	{
		float noise = SimplexNoise(v.texcoord.xy, _NoiseTiling6 * 10);
		uv1 = TilingRemoverVert(_TilingRemover6, uv1, noise);
	}

	if (_TilingRemover7 > 0.0f)
	{
		float noise = SimplexNoise(v.texcoord.xy, _NoiseTiling7 * 10);
		uv2 = TilingRemoverVert(_TilingRemover7, uv2, noise);
	}

	if (_TilingRemover8 > 0.0f)
	{
		float noise = SimplexNoise(v.texcoord.xy, _NoiseTiling8 * 10);
		uv3 = TilingRemoverVert(_TilingRemover8, uv3, noise);
	}

	height1 = tex2Dlod(_Mask0, uv0).b + _HeightShift5;
	height2 = tex2Dlod(_Mask1, uv1).b + _HeightShift6;
	height3 = tex2Dlod(_Mask2, uv2).b + _HeightShift7;
	height4 = tex2Dlod(_Mask3, uv3).b + _HeightShift8;

	float d1 = height1 * splat_control.r * _Displacement5;
	float d2 = height2 * splat_control.g * _Displacement6;
	float d3 = height3 * splat_control.b * _Displacement7;
	float d4 = height4 * splat_control.a * _Displacement8;
	float d = 0;

#ifdef _HEIGHTMAPBLENDING
	d += heightlerp(d2, height2, d1, height1, splat_control.r);
	d += heightlerp(d3, height3, d2, height2, splat_control.g);
	d += heightlerp(d4, height4, d3, height3, splat_control.b);
	d += d4;
#else
	d += d1;
	d += d2;
	d += d3;
	d += d4;

	//float scale = 0.0002;
	//float4 _WorldS = float4(scale, scale, scale, 0);
	//float4 _WorldT = float4(1, 1, 1, 0);
	//float3 worldPos = mul(unity_ObjectToWorld, v.vertex) * _WorldS.xyz + _WorldT.xyz;
	//float3 normal = abs(v.normal);
	//normal /= normal.x + normal.y + normal.z + 1e-3f;
	//
	//d  = splat_control.r * _Displacement1 *
	//(normal.x * tex2Dlod(_Mask0,  float4(_Splat0_ST.xy * worldPos.zy + _Splat0_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xz + _Splat0_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xy + _Splat0_ST.zw, 0, 0))).b;
	//
	//d += splat_control.g * _Displacement2 *
	//(normal.x * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.zy + _Splat1_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xz + _Splat1_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xy + _Splat1_ST.zw, 0, 0))).b;
	//
	//d += splat_control.b * _Displacement3 *
	//(normal.x * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.zy + _Splat2_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xz + _Splat2_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xy + _Splat2_ST.zw, 0, 0))).b;
	//
	//d += splat_control.a * _Displacement4 *
	//(normal.x * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.zy + _Splat3_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xz + _Splat3_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xy + _Splat3_ST.zw, 0, 0))).b;
#endif

//#ifdef _PROCEDURALPUDDLES
	//float noise = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling, 0, 0)).r * _NoiseIntensity;
	//if (t > (1 - _Slope)) d = lerp(d, d - noise - _WaterHeight, _Slope * noise);
//#endif
	
v.color.a = d;

	//fixed splatSum = dot(splat_control, fixed4(1, 1, 1, 1));
	//float dist = distance(_WorldSpaceCameraPos, v.vertex.xyz);
	//d = lerp(t0.a * _Displacement1, t1.a * _Displacement2, t);
	//d -= (1 - splatSum) * dist * 0.005;
	//d -= fadeout;

	v.vertex.xyz += v.normal * d;
#endif

#ifdef _PROCEDURALSNOW
	//float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
	//float4 pos = UnityObjectToClipPos(v.vertex);
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	//float dist = distance(_WorldSpaceCameraPos, v.vertex.xyz);
	half differ = (worldPos.y - _SnowStartHeight);
	half cFactor = saturate(differ / _HeightFalloff / 100);
	v.color.rgb = v.normal;
	v.color = lerp(0, 1, cFactor);

	////float snowDisplacement = tex2Dlod(_SnowDiffuse, float4(v.texcoord.x * _SnowTile, v.texcoord.y * _SnowTile, 0, 0)).a;
	//float snowAmount = (_SnowAmount * (clamp(worldNormal.y + _SnowAngle, 0, 1)) + clamp(v.normal.y + _NormalInfluence, 0, 1) * _SnowAmount * .25) * _SnowPower;
	//snowAmount *= clamp((worldPos.y - _SnowStartHeight) * .0125, 0, 1);
	//snowAmount = clamp(pow(snowAmount , 6) * 256, 0, 1);
	//
	////d = lerp(d, (snowAmount * _SnowThickness) + (d / (_SnowThickness + 1)), snowAmount);
	////if (snowAmount > _SnowDamping) d += snowAmount * _SnowThickness;
	//if (snowAmount > _SnowDamping) d = (snowAmount * _SnowThickness) + (d / ((_SnowThickness / 1) + 1)) - (_SnowThickness * _SnowDamping);
#endif

//#ifdef _TESSELLATION
//float displacement = 0;
//#ifdef _PROCEDURALSNOW
//	displacement = TessellatorWithSnow(tex2Dlod(_Control, float4(v.texcoord.xy, 0, 0)), worldNormal, v.texcoord.xy, _Splat0_ST.xy, _Splat1_ST.xy, _Splat2_ST.xy, _Splat3_ST.xy, _Noise, _NoiseTiling1, _NoiseTiling2, _NoiseTiling3, _NoiseTiling4, _TilingRemover1, _TilingRemover2, _TilingRemover3, _TilingRemover4, _Mask0, _Mask1, _Mask2, _Mask3, v.color.rgb, v.normal, mul(unity_ObjectToWorld, v.vertex), _SnowStartHeight, _HeightFalloff, _SnowAmount, _SnowAngle, _NormalInfluence, _SnowPower, _SnowThickness, _SnowDamping);
//#else
//	displacement = Tessellator(tex2Dlod(_Control, float4(v.texcoord.xy, 0, 0)), worldNormal, v.texcoord.xy, _Splat0_ST.xy, _Splat1_ST.xy, _Splat2_ST.xy, _Splat3_ST.xy, _Noise, _NoiseTiling1, _NoiseTiling2, _NoiseTiling3, _NoiseTiling4, _TilingRemover1, _TilingRemover2, _TilingRemover3, _TilingRemover4, _Mask0, _Mask1, _Mask2, _Mask3);
//#endif
//	v.vertex.xyz += v.normal * displacement;
//	v.color.a = displacement;
//#endif

//#ifdef _NORMALMAP
	v.tangent.xyz = cross(v.normal, float3(0, 0, 1));
	v.tangent.w = -1;
//#endif
}

//inline fixed4 Triplanar(float3 wp, float3 n, float4 st, sampler2D s)
//{
//	return	n.x * UNITY_SAMPLE_TEX2D(s, st.xy * wp.zy + st.zw) +
//			n.y * UNITY_SAMPLE_TEX2D(s, st.xy * wp.xz + st.zw) +
//			n.z * UNITY_SAMPLE_TEX2D(s, st.xy * wp.xy + st.zw);
//}

#if UNITY_VERSION >= 201930
	#ifdef _ALPHATEST_ON
		//sampler2D _TerrainHolesTexture;
		//UNITY_DECLARE_TEX2D_NOSAMPLER(_TerrainHolesTexture);

        void ClipHoles(float2 uv)
        {
            //float hole = tex2D(_TerrainHolesTexture, uv).r;
			//float hole = UNITY_SAMPLE_TEX2D_SAMPLER(_TerrainHolesTexture, _Splat0, uv).r;
			//clip(hole == 0.0f ? -1 : 1);
        }
    #endif
#endif

void SplatmapMix(Input IN, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal, out half _Smoothness, out half _Metallic, out half _AO)
{
#if UNITY_VERSION >= 201930
	#ifdef _ALPHATEST_ON
        ClipHoles(IN.uv_Control);
    #endif
#endif
	
	// adjust splatUVs so the edges of the terrain tile lie on pixel centers
    float2 splatUV = (IN.uv_Control * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    splat_control = tex2D(_Control, splatUV);
	weight = dot(splat_control, half4(1, 1, 1, 1));
	//weight = 1;

//#ifdef _TESSELLATION
//	weight = 1;
//#else
//	weight = dot(splat_control, half4(1, 1, 1, 1));
//
//	#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
//		clip(weight == 0.0f ? -1 : 1);
//	#endif
//#endif

	#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
        clip(weight == 0.0f ? -1 : 1);
	#endif

	splat_control /= (weight + 1e-3f);

	float noise;
	float height1, height2, height3, height4;
	float2 uv0 = splatUV * _Splat0_ST.xy;
	float2 uv1 = splatUV * _Splat1_ST.xy;
	float2 uv2 = splatUV * _Splat2_ST.xy;
	float2 uv3 = splatUV * _Splat3_ST.xy;

	//float noise1 = tex2D(_Noise, splatUV * _NoiseTiling5).r;
	//float noise2 = tex2D(_Noise, splatUV * _NoiseTiling6).r;
	//float noise3 = tex2D(_Noise, splatUV * _NoiseTiling7).r;
	//float noise4 = tex2D(_Noise, splatUV * _NoiseTiling8).r;
	//
	//if(_TilingRemover5 > 0.0f) uv0 = TilingRemoverSurf(_TilingRemover5, uv0, noise1);
	//if(_TilingRemover6 > 0.0f) uv1 = TilingRemoverSurf(_TilingRemover6, uv1, noise2);
	//if(_TilingRemover7 > 0.0f) uv2 = TilingRemoverSurf(_TilingRemover7, uv2, noise3);
	//if(_TilingRemover8 > 0.0f) uv3 = TilingRemoverSurf(_TilingRemover8, uv3, noise4);

	if (_TilingRemover5 > 0.0f)
	{
		noise = SimplexNoise(splatUV, _NoiseTiling5 * 10);
		uv0 = TilingRemoverSurf(_TilingRemover5, uv0, noise);
	}

	if (_TilingRemover6 > 0.0f)
	{
		noise = SimplexNoise(splatUV, _NoiseTiling6 * 10);
		uv1 = TilingRemoverSurf(_TilingRemover6, uv1, noise);
	}

	if (_TilingRemover7 > 0.0f)
	{
		noise = SimplexNoise(splatUV, _NoiseTiling7 * 10);
		uv2 = TilingRemoverSurf(_TilingRemover7, uv2, noise);
	}

	if (_TilingRemover8 > 0.0f)
	{
		noise = SimplexNoise(splatUV, _NoiseTiling8 * 10);
		uv3 = TilingRemoverSurf(_TilingRemover8, uv3, noise);
	}

//	fixed4 t0 = UNITY_SAMPLE_TEX2D(_Splat0, uv0) * _LayerColor5;
//
//#ifdef _HEIGHTMAPBLENDING
//	t0 *= 2;
//#endif
//
//	fixed4 t1 = UNITY_SAMPLE_TEX2D(_Splat1, uv1) * _LayerColor6;
//	fixed4 t2 = UNITY_SAMPLE_TEX2D(_Splat2, uv2) * _LayerColor7;
//	fixed4 t3 = UNITY_SAMPLE_TEX2D(_Splat3, uv3) * _LayerColor8;

	half4 n0 = tex2D(_Normal0, uv0);
	half4 n1 = tex2D(_Normal1, uv1);
	half4 n2 = tex2D(_Normal2, uv2);
	half4 n3 = tex2D(_Normal3, uv3);
	fixed3 _N1 = UnpackNormalWithScale(n0, _NormalScale0) * splat_control.r;
	fixed3 _N2 = UnpackNormalWithScale(n1, _NormalScale1) * splat_control.g;
	fixed3 _N3 = UnpackNormalWithScale(n2, _NormalScale2) * splat_control.b;
	fixed3 _N4 = UnpackNormalWithScale(n3, _NormalScale3) * splat_control.a;
	float4 maskMap1 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap4, _Splat0, uv0);
	float4 maskMap2 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap5, _Splat1, uv1);
	float4 maskMap3 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap6, _Splat2, uv2);
	float4 maskMap4 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap7, _Splat3, uv3);

	proceduralNoiseScale = 4096;
	noise = SimplexNoise(splatUV, proceduralNoiseScale);

	if (proceduralTexturing5 == 1)
		_N1 = lerp(_N1, UnpackNormalWithScale(tex2D(_Normal0, uv0 * 4), _NormalScale0) * splat_control.r, noise);

	if (proceduralTexturing6 == 1)
		_N2 = lerp(_N2, UnpackNormalWithScale(tex2D(_Normal1, uv1 * 4), _NormalScale1) * splat_control.g, noise);

	if (proceduralTexturing7 == 1)
		_N3 = lerp(_N3, UnpackNormalWithScale(tex2D(_Normal2, uv2 * 4), _NormalScale2) * splat_control.b, noise);

	if (proceduralTexturing8 == 1)
		_N4 = lerp(_N4, UnpackNormalWithScale(tex2D(_Normal3, uv3 * 4), _NormalScale3) * splat_control.a, noise);

#ifdef _HEIGHTMAPBLENDING
	if (proceduralTexturing5 == 1)
		height1 = splat_control.r * n0.y;
	else
		height1 = splat_control.r * maskMap1.b;

	if (proceduralTexturing6 == 1)
		height2 = splat_control.g * n1.y;
	else
		height2 = splat_control.g * maskMap2.b;

	if (proceduralTexturing7 == 1)
		height3 = splat_control.b * n2.y;
	else
		height3 = splat_control.b * maskMap3.b;

	if (proceduralTexturing8 == 1)
		height4 = splat_control.a * n3.y;
	else
		height4 = splat_control.a * maskMap4.b;

	mixedNormal  = heightlerp(_N2, height2, _N1, height1, splat_control.r);
	mixedNormal += heightlerp(_N3, height3, _N2, height2, splat_control.g);
	mixedNormal += heightlerp(_N4, height4, _N3, height3, splat_control.b);
	mixedNormal += _N4;
#else
	mixedNormal  = _N1;
	mixedNormal += _N2;
	mixedNormal += _N3;
	mixedNormal += _N4;
#endif

	mixedNormal.z += 1e-5f; // to avoid nan after normalizing
	
	half3 _worldNormal = WorldNormalVector(IN, mixedNormal);
	half slope = 1 - dot(_worldNormal, normalize(half3(0, 1, 0)));
	fixed4 t0, t1, t2, t3;

	if (proceduralTexturing5 == 1)
	{
		if (gradientTexturing5 == 1)
			t0 = UNITY_SAMPLE_TEX2D(_Splat0, IN.worldPos.xy * gradientTiling5 * 0.01) * lerp(_LayerColor5, _LayerColor5 * noise, n0.y);
			//t0 = UNITY_SAMPLE_TEX2D(_Splat0, IN.worldPos.xy * 0.1) * lerp(_LayerColor5, _LayerColor5 * noise * 1.5, noise);
		else
			t0 = UNITY_SAMPLE_TEX2D(_Splat0, uv0) * lerp(_LayerColor5, _LayerColor5 * noise, n0.y);

		t0 = (lerp(t0, pow(_LayerColor5, 64) + colorDamping5, slope * slopeInfluence5));
		t0 = lerp(t0, _LayerColor5 - 0.1, pow(1 - n0.y, 5));
	}
	else
		t0 = UNITY_SAMPLE_TEX2D(_Splat0, uv0) * _LayerColor5;

	if (proceduralTexturing6 == 1)
	{
		if (gradientTexturing6 == 1)
			t1 = UNITY_SAMPLE_TEX2D(_Splat1, IN.worldPos.xy * gradientTiling6 * 0.01) * lerp(_LayerColor6, _LayerColor6 * noise, n1.y);
		else
			t1 = UNITY_SAMPLE_TEX2D(_Splat1, uv1) * lerp(_LayerColor6, _LayerColor6 * noise, n1.y);

		t1 = (lerp(t1, pow(_LayerColor6, 64) + colorDamping6, slope * slopeInfluence6));
		t1 = lerp(t1, _LayerColor6 - 0.1, pow(1 - n0.y, 5));
	}
	else
		t1 = UNITY_SAMPLE_TEX2D(_Splat1, uv1) * _LayerColor6;

	if (proceduralTexturing7 == 1)
	{
		if (gradientTexturing7 == 1)
			t2 = UNITY_SAMPLE_TEX2D(_Splat2, IN.worldPos.xy * gradientTiling7 * 0.01) * lerp(_LayerColor7, _LayerColor7 * noise, n2.y);
		else
			t2 = UNITY_SAMPLE_TEX2D(_Splat2, uv2) * lerp(_LayerColor7, _LayerColor7 * noise, n2.y);

		t2 = (lerp(t2, pow(_LayerColor7, 64) + colorDamping7, slope * slopeInfluence7));
		t2 = lerp(t2, _LayerColor7 - 0.1, pow(1 - n0.y, 5));
	}
	else
		t2 = UNITY_SAMPLE_TEX2D(_Splat2, uv2) * _LayerColor7;

	if (proceduralTexturing8 == 1)
	{
		if (gradientTexturing8 == 1)
			t3 = UNITY_SAMPLE_TEX2D(_Splat3, IN.worldPos.xy * gradientTiling8 * 0.01) * lerp(_LayerColor8, _LayerColor8 * noise, n3.y);
		else
			t3 = UNITY_SAMPLE_TEX2D(_Splat3, uv3) * lerp(_LayerColor8, _LayerColor8 * noise, n3.y);

		t3 = (lerp(t3, pow(_LayerColor8, 64) + colorDamping8, slope * slopeInfluence8));
		t3 = lerp(t3, _LayerColor8 - 0.1, pow(1 - n0.y, 5));
	}
	else
		t3 = UNITY_SAMPLE_TEX2D(_Splat3, uv3) * _LayerColor8;
	
	fixed4 _Diffuse1 = splat_control.r * t0; // * half4(1.0, 1.0, 1.0, maskMap1.a);
	fixed4 _Diffuse2 = splat_control.g * t1; // * half4(1.0, 1.0, 1.0, maskMap2.a);
	fixed4 _Diffuse3 = splat_control.b * t2; // * half4(1.0, 1.0, 1.0, maskMap3.a);
	fixed4 _Diffuse4 = splat_control.a * t3; // * half4(1.0, 1.0, 1.0, maskMap4.a);

#ifdef _HEIGHTMAPBLENDING
	mixedDiffuse  = heightlerp(_Diffuse2, height2, _Diffuse1, height1, splat_control.r);
	mixedDiffuse += heightlerp(_Diffuse3, height3, _Diffuse2, height2, splat_control.g);
	mixedDiffuse += heightlerp(_Diffuse4, height4, _Diffuse3, height3, splat_control.b);
	mixedDiffuse += _Diffuse4;
#else
	mixedDiffuse  = _Diffuse1;
	mixedDiffuse += _Diffuse2;
	mixedDiffuse += _Diffuse3;
	mixedDiffuse += _Diffuse4;
#endif

	_Metallic  = splat_control.r * maskMap1.r * _Metallic0;
	_Metallic += splat_control.g * maskMap2.r * _Metallic1;
	_Metallic += splat_control.b * maskMap3.r * _Metallic2;
	_Metallic += splat_control.a * maskMap4.r * _Metallic3;
	
	if (proceduralTexturing5 == 1)
		_Smoothness = splat_control.r * _Smoothness0 * pow(1 - n0.y, 4);
	else
		_Smoothness = splat_control.r * maskMap1.a * _Smoothness0;

	if (proceduralTexturing6 == 1)
		_Smoothness += splat_control.g * _Smoothness1 * pow(1 - n1.y, 4);
	else
		_Smoothness += splat_control.g * maskMap2.a * _Smoothness1;

	if (proceduralTexturing7 == 1)
		_Smoothness += splat_control.b * _Smoothness2 * pow(1 - n2.y, 4);
	else
		_Smoothness += splat_control.b * maskMap3.a * _Smoothness2;

	if (proceduralTexturing8 == 1)
		_Smoothness += splat_control.a * _Smoothness3 * pow(1 - n3.y, 4);
	else
		_Smoothness += splat_control.a * maskMap4.a * _Smoothness3;

	//_AO = 1;

	if (proceduralTexturing5 == 1)
		_AO = splat_control.r * _LayerAO5 * (1 - n0.y) * 2;
	else
		_AO = splat_control.r * maskMap1.g * _LayerAO5;

	if (proceduralTexturing6 == 1)
		_AO += splat_control.g * _LayerAO6 * (1 - n1.y) * 2;
	else
		_AO += splat_control.g * maskMap2.g * _LayerAO6;

	if (proceduralTexturing7 == 1)
		_AO += splat_control.b * _LayerAO7 * (1 - n2.y) * 2;
	else
		_AO += splat_control.b * maskMap3.g * _LayerAO7;

	if (proceduralTexturing8 == 1)
		_AO += splat_control.a * _LayerAO8 * (1 - n3.y) * 2;
	else
		_AO += splat_control.a * maskMap4.g * _LayerAO8;

//	fixed4 _Diffuse1 = splat_control.r * t0; // * half4(1.0, 1.0, 1.0, maskMap1.a);
//	fixed4 _Diffuse2 = splat_control.g * t1; // * half4(1.0, 1.0, 1.0, maskMap2.a);
//	fixed4 _Diffuse3 = splat_control.b * t2; // * half4(1.0, 1.0, 1.0, maskMap3.a);
//	fixed4 _Diffuse4 = splat_control.a * t3; // * half4(1.0, 1.0, 1.0, maskMap4.a);
//
//#ifdef _HEIGHTMAPBLENDING
//	height1 = splat_control.r * maskMap1.b;
//	height2 = splat_control.g * maskMap2.b;
//	height3 = splat_control.b * maskMap3.b;
//	height4 = splat_control.a * maskMap4.b;
//	mixedDiffuse  = heightlerp(_Diffuse2, height2, _Diffuse1, height1, splat_control.r);
//	mixedDiffuse += heightlerp(_Diffuse3, height3, _Diffuse2, height2, splat_control.g);
//	mixedDiffuse += heightlerp(_Diffuse4, height4, _Diffuse3, height3, splat_control.b);
//	mixedDiffuse += _Diffuse4;
//#else
//	mixedDiffuse  = _Diffuse1;
//	mixedDiffuse += _Diffuse2;
//	mixedDiffuse += _Diffuse3;
//	mixedDiffuse += _Diffuse4;
//
//	//half4 n0 = tex2D(_Normal0, uv0);
//	//half4 n1 = tex2D(_Normal1, uv1);
//	//half4 n2 = tex2D(_Normal2, uv2);
//	//half4 n3 = tex2D(_Normal3, uv3);
//	//
//	//fixed3 _Normal1 = UnpackNormalWithScale(n0, _NormalScale0) * splat_control.r;
//	//fixed3 _Normal2 = UnpackNormalWithScale(n1, _NormalScale1) * splat_control.g;
//	//fixed3 _Normal3 = UnpackNormalWithScale(n2, _NormalScale2) * splat_control.b;
//	//fixed3 _Normal4 = UnpackNormalWithScale(n3, _NormalScale3) * splat_control.a;
//	//fixed3 nrm  = _Normal1;
//	//nrm += _Normal2;
//	//nrm += _Normal3;
//	//nrm += _Normal4;
//	//
//	//float3 worldNormal = WorldNormalVector(IN, nrm);
//	//
//	//float scale = 0.0002;
//	//float4 _WorldS = float4(scale, scale, scale, 0);
//	//float4 _WorldT = float4(1, 1, 1, 0);
//	//float3 worldPos = IN.worldPos * _WorldS.xyz + _WorldT.xyz;
//	//float3 normal = abs(IN.color);
//	//normal /= normal.x + normal.y + normal.z + 1e-3f;
//	//
//	//mixedDiffuse  = splat_control.r *
//	//(normal.x * UNITY_SAMPLE_TEX2D(_Splat0, _Splat0_ST.xy * worldPos.zy + _Splat0_ST.zw) +
//	//normal.y  * UNITY_SAMPLE_TEX2D(_Splat0, _Splat0_ST.xy * worldPos.xz + _Splat0_ST.zw) +
//	//normal.z  * UNITY_SAMPLE_TEX2D(_Splat0, _Splat0_ST.xy * worldPos.xy + _Splat0_ST.zw));
//	//
//	//mixedDiffuse += splat_control.g *
//	//(normal.x * UNITY_SAMPLE_TEX2D(_Splat1, _Splat1_ST.xy * worldPos.zy + _Splat1_ST.zw) +
//	//normal.y  * UNITY_SAMPLE_TEX2D(_Splat1, _Splat1_ST.xy * worldPos.xz + _Splat1_ST.zw) +
//	//normal.z  * UNITY_SAMPLE_TEX2D(_Splat1, _Splat1_ST.xy * worldPos.xy + _Splat1_ST.zw));
//	//
//	//mixedDiffuse += splat_control.b *
//	//(normal.x * UNITY_SAMPLE_TEX2D(_Splat2, _Splat2_ST.xy * worldPos.zy + _Splat2_ST.zw) +
//	//normal.y  * UNITY_SAMPLE_TEX2D(_Splat2, _Splat2_ST.xy * worldPos.xz + _Splat2_ST.zw) +
//	//normal.z  * UNITY_SAMPLE_TEX2D(_Splat2, _Splat2_ST.xy * worldPos.xy + _Splat2_ST.zw));
//	//
//	//mixedDiffuse += splat_control.a *
//	//(normal.x * UNITY_SAMPLE_TEX2D(_Splat3, _Splat3_ST.xy * worldPos.zy + _Splat3_ST.zw) +
//	//normal.y  * UNITY_SAMPLE_TEX2D(_Splat3, _Splat3_ST.xy * worldPos.xz + _Splat3_ST.zw) +
//	//normal.z  * UNITY_SAMPLE_TEX2D(_Splat3, _Splat3_ST.xy * worldPos.xy + _Splat3_ST.zw));
//#endif
//
////#ifdef _NORMALMAP
//	half4 n0 = tex2D(_Normal0, uv0);
//	half4 n1 = tex2D(_Normal1, uv1);
//	half4 n2 = tex2D(_Normal2, uv2);
//	half4 n3 = tex2D(_Normal3, uv3);
//
//	fixed3 _Normal1 = UnpackNormalWithScale(n0, _NormalScale0) * splat_control.r;
//	fixed3 _Normal2 = UnpackNormalWithScale(n1, _NormalScale1) * splat_control.g;
//	fixed3 _Normal3 = UnpackNormalWithScale(n2, _NormalScale2) * splat_control.b;
//	fixed3 _Normal4 = UnpackNormalWithScale(n3, _NormalScale3) * splat_control.a;
//
//	#ifdef _HEIGHTMAPBLENDING
//		mixedNormal  = heightlerp(_Normal2, height2, _Normal1, height1, splat_control.r);
//		mixedNormal += heightlerp(_Normal3, height3, _Normal2, height2, splat_control.g);
//		mixedNormal += heightlerp(_Normal4, height4, _Normal3, height3, splat_control.b);
//		mixedNormal += _Normal4;
//	#else
//		mixedNormal  = _Normal1;
//		mixedNormal += _Normal2;
//		mixedNormal += _Normal3;
//		mixedNormal += _Normal4;
//	#endif
//
//	mixedNormal.z += 1e-5f; // to avoid nan after normalizing
////#endif
}

#ifndef TERRAIN_SURFACE_OUTPUT
	#define TERRAIN_SURFACE_OUTPUT SurfaceOutput
#endif

void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
{
	color *= o.Alpha;

#ifdef TERRAIN_SPLAT_ADDPASS
	UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
#else
	UNITY_APPLY_FOG(IN.fogCoord, color);
#endif
}

void SplatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 normalSpec)
{
	normalSpec *= o.Alpha;
}

void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission)
{
	UnityStandardDataApplyWeightToGbuffer(outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha);
	emission *= o.Alpha;
}

void surf (Input IN, inout SurfaceOutputStandard o)
{
    half4 splat_control;
    half weight;
    fixed4 mixedDiffuse;
	half _Metallic;
	half _Smoothness;
	half _AO;
	SplatmapMix(IN, splat_control, weight, mixedDiffuse, o.Normal, _Smoothness, _Metallic, _AO);

	fixed3 finalCol;

#ifdef _FLATSHADING
	FlatShadingFrag(o.Normal, IN.worldPos.xyz, WorldNormalVector(IN, half3(1, 0, 0)), WorldNormalVector(IN, half3(0, 1, 0)), WorldNormalVector(IN, half3(0, 0, 1)));
#endif

#ifdef _COLORMAPBLENDING
	finalCol = ColormapBlending(IN.uv_Control, _WorldSpaceCameraPos, IN.worldPos, mixedDiffuse, _LightingColor);
#else
	finalCol = mixedDiffuse * _LightingColor;
#endif

half3 _worldNormal = WorldNormalVector(IN, o.Normal);
float waterMask = 0;

#if defined(_PROCEDURALPUDDLES) || defined(_PROCEDURALSNOW)
	waterMask = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_WaterMask, _Splat0, IN.uv_Control).a;
#endif

#ifdef _PROCEDURALPUDDLES
	half slope = 1 - dot(_worldNormal, normalize(half3(0, 1, 0)));

	//float noise = tex2D(_Noise, IN.uv_Control * _NoiseTiling).r;
	float noise = 1.0 - SimplexNoise(IN.uv_Control, _NoiseTiling * 10);

	half puddle = slope * noise;
	//puddle -= splat_control.gba;
	Puddles(puddle, o.Albedo, o.Normal, o.Smoothness, o.Metallic, _Smoothness, _Metallic, finalCol.rgb, waterMask, 3);

	#ifdef _PROCEDURALSNOW
		if (puddle > _Slope || puddle < _SlopeMin)
		{
			SnowWithFlow(o.Albedo, o.Smoothness, o.Normal, _worldNormal, IN.uv_Control, IN.color.y, IN.worldPos.y, finalCol.rgb, _Smoothness, waterMask, splat_control);
			o.Metallic = _Metallic;
		}
	#endif
#else
	#ifdef _PROCEDURALSNOW
		SnowWithFlow(o.Albedo, o.Smoothness, o.Normal, _worldNormal, IN.uv_Control, IN.color.y, IN.worldPos.y, finalCol.rgb, _Smoothness, waterMask, splat_control);
	#else
		o.Albedo = finalCol;
		o.Smoothness = _Smoothness;
	#endif

	o.Metallic = _Metallic;
#endif

	o.Occlusion = _AO;
	o.Alpha = weight;
}

#endif // TERRAFORMER_SPLATMAPCOMMON_ADD_CGINC_INCLUDED

