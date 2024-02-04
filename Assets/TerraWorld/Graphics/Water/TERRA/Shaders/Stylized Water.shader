Shader "TerraUnity/Water Stylized"
{
	Properties
	{
		_MainColor("Main Color", COLOR) = (.34, .85, .92, 1)
		_RefrColor("Refraction Color", COLOR) = (.34, .85, .92, 1)
		 [Space(20)]

		_PatterScale("Pattern Scale", Range(0.02,0.5)) = 0.03
        _WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	    _SpeedMultiply("Speed Multiply",  Range(1,10)) = 1
	     [Space(20)]

		_ReflDistort("Reflection Distort", Range(0,5)) = 1
		_RefrDistort("Refraction Distort", Range(0,5)) = 1
		[NoScaleOffset] _Fresnel("Fresnel (A) ", 2D) = "gray" {}

		[Space(20)]
		[NoScaleOffset] _BumpMap("Normalmap ", 2D) = "bump" {}
		_NormalPower1("Normal Power", Range(0,4)) = 0.5
	    _InvertNormal("Invert Normal", Range(-1,1)) = 1

	    [Space(20)]
		[NoScaleOffset]_MaskTex("Mask Image", 2D) = "" {}
		_Glossiness("Glossiness", Range(0,10)) = 0.5

		[NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "grey" {}
	}

	// -----------------------------------------------------------
	// Fragment program cards

	Subshader
	{
		Tags { "WaterMode" = "Refractive" "RenderType" = "Opaque" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			float _PatterScale;
			uniform float4 _PatterScale4;
			uniform float4 _PatterOffset;
			float4 _WaveSpeed;
			float _SpeedMultiply;
			uniform float4 Time;
			uniform float _ReflDistort;
			uniform float _RefrDistort;
			samplerCUBE _Tex;
			half4 _Tex_HDR;
			float4 _MainColor;

			struct Input
			{
			
				float4 _Time;
			};

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 ref : TEXCOORD0;
				float2 bumpuv0 : TEXCOORD1;
				float2 bumpuv1 : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				UNITY_FOG_COORDS(4)
			};

			v2f vert(appdata v)
			{
				_Time = float4(_Time.r * _SpeedMultiply, _Time.g * _SpeedMultiply, _Time.b * _SpeedMultiply, _Time.a * _SpeedMultiply);
				_PatterScale4 = float4(_PatterScale, _PatterScale, _PatterScale * 0.4f, _PatterScale * 0.45f);
				_PatterOffset = float4(_WaveSpeed.r*_Time.r / 1000, _WaveSpeed.g *_Time.g / 1000, _WaveSpeed.b* _Time.b / 1000, _WaveSpeed.a* _Time.a / 1000);

				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				// scroll Normal waves
				float4 temp;
				float4 wpos = mul(unity_ObjectToWorld, v.vertex);
				temp.xyzw = wpos.xzxz * _PatterScale4 + _PatterOffset;
				o.bumpuv0 = temp.xy;
				o.bumpuv1 = temp.wz;

				// object space view direction (will normalize per pixel)
				o.viewDir.xzy = WorldSpaceViewDir(v.vertex);

				o.ref = ComputeScreenPos(o.pos);

				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			sampler2D _MaskTex;
			sampler2D _Fresnel;
			sampler2D _RefractionTex;
			sampler2D _BumpMap;
			uniform float4 _RefrColor;
			float _NormalPower1;
			float _InvertNormal;
			half _Glossiness;

			half4 frag(v2f i) : SV_Target
			{
				i.viewDir = normalize(i.viewDir);

				// combine two scrolling bumpmaps into one
				half3 bump1 = UnpackNormal(tex2D(_BumpMap, i.bumpuv0)).rgb;
				half3 bump2 = UnpackNormal(tex2D(_BumpMap, i.bumpuv1)).rgb;
				half3 bump = (bump1 + bump2) * _NormalPower1 * _InvertNormal;
	
				// fresnel factor
				half fresnelFac = dot(i.viewDir, bump);

				// perturb reflection/refraction UVs by bumpmap, and lookup colors

				float4 uv1 = i.ref; uv1.xy += bump * _ReflDistort;
				half4 refl = tex2Dproj(_MaskTex , UNITY_PROJ_COORD(uv1));
			
				float4 uv2 = i.ref; uv2.xy -= bump * _RefrDistort;
				half4 refr = tex2Dproj(_MaskTex , UNITY_PROJ_COORD(uv2)) * _RefrColor;

				half4 tex = texCUBE(_Tex, i.ref);
				half4 c = half4( DecodeHDR(tex, _Tex_HDR),1);
				// final color is between refracted and reflected based on fresnel
				half4 color;
			
				half fresnel = UNITY_SAMPLE_1CHANNEL(_Fresnel, float2(fresnelFac,fresnelFac));
				color = lerp(refr, float4(refl.rgb * _Glossiness,1), fresnel);

				c = c * color.rgba * float4(unity_ColorSpaceDouble.rgb,1);
				color *= c;
				
				color *= _MainColor;
				//texCube ()
				UNITY_APPLY_FOG(i.fogCoord, color);
				return color;
			}
			ENDCG
		}
	}
}

