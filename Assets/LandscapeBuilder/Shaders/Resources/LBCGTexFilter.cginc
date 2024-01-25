// Landscape Builder. Copyright (c) 2016-2020 SCSM Pty Ltd. All rights reserved.
// Common LBFilter methods for Texture filters. Used in LBCSGrass.compute shader

// Splatmap variables
uint numSplatTextures;
uint alphaMapWidth;
uint alphaMapLength;
StructuredBuffer<float> splatMaps;

// Returns 0.0 if the point has been filtered out.
// Returns 1.0 if the point has not been filtered out.
float TextureFilterIncluded(uint smTexIdx, float2 posN, int filterMode, float cutOff)
{
	float isIncluded = 1.0; // Filter has no effect by default

	float blendWeight = splatMaps[(alphaMapWidth * alphaMapLength * smTexIdx) + (alphaMapWidth * uint(posN.y * ((float)alphaMapLength-1.0))) + uint(posN.x * ((float)alphaMapWidth-1.0))];

	if (filterMode == 2) // NOT
	{
		if (blendWeight >= cutOff) { isIncluded = 0.0; }
	}
	else // AND - OR is currently not available
	{
		if (blendWeight < cutOff) { isIncluded = 0.0; }
	}

	return isIncluded;
}
