#ifndef TERRASTANDARD_LIGHTING_CGINC_INCLUDED
#define TERRASTANDARD_LIGHTING_CGINC_INCLUDED

sampler2D _Ramp;

float4 LightingRealWorld(float3 normal, float smoothness, float3 diffuseColor, float3 lightDir, float atten)
{
	// How much does the normal point towards the light?
	float towardsLight = dot(normal, lightDir);

	// Remap the value from -1 to 1 to between 0 and 1
	towardsLight = towardsLight * 0.5 + 0.5;

	// Read from ramp
	float3 lightIntensity = tex2D(_Ramp, towardsLight).rgb;

	//Specular calculations
	float3 specularity = pow(towardsLight * smoothness, 3) * pow(_LightColor0.rgb + diffuseColor, 0.25);

	// Combine the color
	float4 col;

	// Intensity we calculated previously, diffuse color, light falloff and shadowcasting, color of the light
	col.rgb = (lightIntensity * diffuseColor + specularity) * atten * _LightColor0.rgb;

	// In case we want to make the shader transparent in the future - irrelevant right now
	col.a = 1; // 1

	return col;

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

#endif // TERRASTANDARD_LIGHTING_CGINC_INCLUDED

