#ifndef TERRAFORMER_LIGHTING_CGINC_INCLUDED
#define TERRAFORMER_LIGHTING_CGINC_INCLUDED

sampler2D _Ramp;

float4 LightingRealWorld(SurfaceOutputStandard s, float3 lightDir, float atten)
{
#ifdef _FLATSHADING
	// Flat Shading Lighting
	float _LightSteps = 16;
	float NdotL = max(0.0, dot(s.Normal, lightDir));
	float lightBandsMultiplier = _LightSteps / 256;
	float lightBandsAdditive = _LightSteps / 2;
	fixed bandedNdotL = (floor((NdotL * 256 + lightBandsAdditive) / _LightSteps)) * lightBandsMultiplier;
	float3 lightIntensity = tex2D(_Ramp, NdotL).rgb; // Read from ramp
	float3 specularity = pow(NdotL * s.Smoothness, 3) * pow(_LightColor0.rgb + s.Albedo, 0.25); //Specular calculations
	float4 col; // Combine the color
	col.rgb = (lightIntensity * bandedNdotL * s.Albedo + specularity) * atten * _LightColor0.rgb; // Intensity we calculated previously, diffuse color, light falloff and shadowcasting, color of the light
	col.a = s.Alpha; // 1 - In case we want to make the shader transparent in the future - irrelevant right now
	return col;
#else
	// Realistic Smooth Lighting
	float towardsLight = dot(s.Normal, lightDir); // How much does the normal point towards the light?
	towardsLight = towardsLight * 0.5 + 0.5; // Remap the value from -1 to 1 to between 0 and 1
	float3 lightIntensity = tex2D(_Ramp, towardsLight).rgb; // Read from ramp
	float3 specularity = pow(towardsLight * s.Smoothness, 3) * pow(_LightColor0.rgb + s.Albedo, 0.25); //Specular calculations
	float4 col; // Combine the color
	col.rgb = (lightIntensity * s.Albedo + specularity) * atten * _LightColor0.rgb; // Intensity we calculated previously, diffuse color, light falloff and shadowcasting, color of the light
	col.a = s.Alpha; // 1 - In case we want to make the shader transparent in the future - irrelevant right now
	return col;
#endif

	// Blinn-Phong
	//float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	//float3 halfDirection = normalize(viewDirection + lightDirection);
	//float NdotL = max(0, dot(normalDirection, lightDirection));
	//float NdotV = max(0, dot(normalDirection, halfDirection));
	//
	////Specular calculations
	//float3 specularity = pow(NdotV, _SpecularGloss) * _SpecularPower * _SpecularColor.rgb;
	//float3 lightingModel = NdotL * diffuseColor + specularity;
	//
	//float attenuation = LIGHT_ATTENUATION(i);
	//float3 attenColor = attenuation * _LightColor0.rgb;
	//float4 finalDiffuse = float4(lightingModel * attenColor, 1);
}

// Ref.
fixed4 LightingOriginal(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
{
    SurfaceOutputStandard r;
    r.Albedo = s.Albedo;
    r.Normal = s.Normal;
    r.Emission = s.Emission;
    r.Metallic = s.Metallic;
    r.Smoothness = s.Smoothness;
    r.Occlusion = s.Occlusion;
    r.Alpha = s.Alpha;
    return LightingStandard(r, viewDir, gi);
}

inline void LightingOriginal_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
{
    UNITY_GI(gi, s, data);
}

#endif // TERRAFORMER_LIGHTING_CGINC_INCLUDED

