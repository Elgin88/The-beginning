#ifndef TERRASTANDARD_SATELLITEIMAGEBLENDING_CGINC_INCLUDED
#define TERRASTANDARD_SATELLITEIMAGEBLENDING_CGINC_INCLUDED

sampler2D _Colormap;
float4    _Colormap_ST;
float     _ColormapInfluence;
float4    _WorldSize;

void SatelliteImageBlending (inout half3 diffColor, float3 modelUV, float2 worldUV, sampler2D _Colormap)
{
    //TODO: Implement UV-free texturing based on i.pos.y
    float fadeout = saturate(pow(modelUV.y, _ColormapInfluence));

    
#if !defined (_ALPHATEST_ON) // Rendering Type - opaque (e.g. Bark)
    float flattenUV = 0.5; // We use flattened UVs for uniform color distribution on the whole model
    _ColormapInfluence *= 0.666; // If material is opaque, apply 2/3 of intensity (_ColormapInfluence) which is more suited for non-transparent objects
	fadeout = saturate(pow(flattenUV, _ColormapInfluence));
#endif

    float4 colormap = tex2D(_Colormap, worldUV);
    diffColor = lerp(diffColor * colormap, diffColor, fadeout);
}

#endif // TERRASTANDARD_SATELLITEIMAGEBLENDING_CGINC_INCLUDED

