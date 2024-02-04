#ifndef TERRASTANDARD_WIND_CGINC_INCLUDED
#define TERRASTANDARD_WIND_CGINC_INCLUDED

// Generic Params
float _WindState;

// Leaves & Bark
float3 _BaseWindMultipliers; // x: main, y: turbulence, z: flutter
float _TumbleStrength;
float _TumbleFrequency;
float _TimeOffset;
float _LeafTurbulence;
float _EdgeFlutterInfluence;
float2 _AdvancedEdgeBending;
float4 _TerrainLODWind;
float _WindSpeedMultiplier;
float _WindSpeedMultiplierGrass;
float _FadeOutAllLeaves;
float _FadeOutWind;

// Needs to be static or else it won't be validated!
static float stiffness = 0.7;

// Grass
float _ShakeTime;
float _ShakeWindspeed;
float _ShakeBending;

// Touch Bending Params
uniform float4 _BendData[16];
uniform float _BendIntensity;
//uniform float3 worldPos;
//uniform float3 sphereDisp;


// Generic Functions

void FastSinCos(float4 val, out float4 s, out float4 c)
{
	val = val * 6.408849 - 3.1415927;
	float4 r5 = val * val;
	float4 r6 = r5 * r5;
	float4 r7 = r6 * r5;
	float4 r8 = r6 * r5;
	float4 r1 = r5 * val;
	float4 r2 = r1 * r5;
	float4 r3 = r2 * r5;
	float4 sin7 = { 1, -0.16161616, 0.0083333, -0.00019841 };
	float4 cos8 = { -0.5, 0.041666666, -0.0013888889, 0.000024801587 };
	s = val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
	c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}

float4 SmoothCurve(float4 x)
{
	return x * x * (3.0 - 2.0 * x);
}

float4 TriangleWave(float4 x)
{
	return abs(frac(x + 0.5) * 2.0 - 1.0);
}

float4 SmoothTriangleWave(float4 x)
{
	return SmoothCurve(TriangleWave(x));
}

float4 AfsSmoothTriangleWave(float4 x)
{
	return (SmoothCurve(TriangleWave(x)) - 0.5f) * 2.0f;
}

#define FLT_MAX 3.402823466e+38 // Maximum representable floating-point number
float3 FastSign(float3 x)
{
	return saturate(x * FLT_MAX + 0.5f) * 2.0f - 1.0f;
}

// see http://www.neilmendoza.com/glsl-rotation-about-an-arbitrary-axis/
// 13fps
float3x3 AfsRotationMatrix(float3 axis, float angle)
{
	//axis = normalize(axis); // moved to calling function
	float s = sin(angle);
	float c = cos(angle);
	float oc = 1.0f - c;

	return float3x3	(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
		oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
		oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
}

float Pingpong()
{
	int remainder = fmod(floor(_Time.y * _Time.x), 2);
	return remainder == 1?1 - frac(_Time.y * _Time.x) : frac(_Time.y * _Time.x);
}

void TouchBending (inout float3 vertex, float texCoordY)
{
	for (int i = 0; i < 16; i++)
	{
		//float bendRadius = _BendData[i].w;
		//float3 benderWorldPos = _BendData[i].xyz;
		//
		////float3 vertexWorldPos = mul(unity_ObjectToWorld, vertex);
		//float3 vertexWorldPos = mul(unity_ObjectToWorld, float4(vertex.xyz, 1)).xyz;
		//vertexWorldPos.z += 1;
		//
		//float distToBender = distance(vertexWorldPos, benderWorldPos);
		////float bendPower = (bendRadius - min(bendRadius, distToBender)) / (bendRadius + 0.001) * 10;
		//
		//float3 circle = 1 - saturate(distToBender / bendRadius);
		//float3 bendDir = normalize(vertexWorldPos - benderWorldPos);
		//bendDir *= circle;
		//
		//float2 vertexOffset = bendDir.xz * _BendIntensity * texCoordY * tangentY;
		//
		////v.vertex.xz += lerp(float2(0, 0), vertexOffset, saturate(bendRadius * v.color.w));
		//vertex.xz += lerp(float2(0, 0), vertexOffset, bendRadius);

		//float3 worldPos = mul(unity_ObjectToWorld, vertex);
		float3 worldPos = mul(unity_ObjectToWorld, float4(vertex.xyz, 1)).xyz;
		worldPos.z += 1;

		float _Radius = _BendData[i].w;
		float3 dist = distance(worldPos, _BendData[i].xyz);
		float3 circle = 1 - saturate(dist / _Radius);
		float3 sphereDisp = worldPos - _BendData[i].xyz;
		sphereDisp *= circle;
		vertex.xz += sphereDisp.xz * (_BendIntensity + (Pingpong() * _BendIntensity / 16)) * texCoordY;
	}
}

// Wind Functions

void WindSimulationLeavesAndBark(bool isLeavesRendering, float4 pos, float4 animParams, float3 pivot, float tumbleInfluence, float4 Wind, float packedBranchAxis, inout float3 _normal, inout float4 _vertex, inout float4 _vertexColor, float texCoordY)
{
	if (_WindState == 1)
	{
		//animParams.x = branch phase
		//animParams.y = edge flutter factor
		//animParams.z = primary factor UV2.x
		//animParams.w = secondary factor UV2.y

		for (int i = 0; i < 16; i++)
		{
			float3 worldPos = mul(unity_ObjectToWorld, float4(_vertex.xyz, 1)).xyz;
			worldPos.z += 1;
			float _Radius = _BendData[i].w;
			float3 dist = distance(worldPos, _BendData[i].xyz);
			float3 circle = 1 - saturate(dist / _Radius);
			float3 sphereDisp = worldPos - _BendData[i].xyz;
			sphereDisp *= circle;
			animParams.xy += sphereDisp.xz * _BendIntensity * 6;
			tumbleInfluence += sphereDisp.xz * _BendIntensity * 6;
		}

		//	Adjust base wind settings
		animParams.zwy *= _BaseWindMultipliers;

		float fDetailAmp = 0.1f;
		float fBranchAmp = 0.3f; // 0.3f;

		float fade = (_FadeOutWind == 1 && unity_LODFade.x > 0) ? unity_LODFade.x : 1.0;

		//	Add extra animation to make it fit speedtree
		float3 TreeWorldPos = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);

		//	fern issue / this does not seem to fix the problem... / float3 TreeWorldPos = mul(unity_ObjectToWorld, float4(0,0,0,1));
		TreeWorldPos.xyz = abs(TreeWorldPos.xyz * 0.125f);
		float sinuswave = _SinTime.z;
		//	float4 vOscillations = AfsSmoothTriangleWave(float4(TreeWorldPos.x + sinuswave , TreeWorldPos.z + sinuswave * 0.8, 0.0, 0.0));

//#if defined (LEAFTUMBLING)
		float shiftedsinuswave = sin(_Time.y * 0.5 + _TimeOffset);
		float4 vOscillations = AfsSmoothTriangleWave(float4(TreeWorldPos.x + sinuswave, TreeWorldPos.z + sinuswave * 0.7, TreeWorldPos.x + shiftedsinuswave, TreeWorldPos.z + shiftedsinuswave * 0.8));
//#else
		//float4 vOscillations = AfsSmoothTriangleWave(float4(TreeWorldPos.x + sinuswave, TreeWorldPos.z + sinuswave * 0.7, 0.0, 0.0));
//#endif

		// vOscillations.xz = lerp(vOscillations.xz, 1, vOscillations.xz );
		// x used for main wind bending / y used for tumbling
		float2 fOsc = vOscillations.xz + (vOscillations.yw * vOscillations.yw);
		fOsc = 0.75 + (fOsc + 3.33) * 0.33;

		Wind *= fade.xxxx;

		float absWindStrength = length(Wind.xyz);

		// Phases (object, vertex, branch)
		// float fObjPhase = dot(unity_ObjectToWorld[3].xyz, 1);
		// new
		float fObjPhase = abs(frac((TreeWorldPos.x + TreeWorldPos.z) * 0.5) * 2 - 1);
		float fBranchPhase = fObjPhase + animParams.x;
		float fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);

		// x is used for edges; y is used for branches
		float2 vWavesIn = _Time.yy + float2(fVtxPhase, fBranchPhase);

		// 1.975, 0.793, 0.375, 0.193 are good frequencies
		float4 vWaves = (frac(vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193)) * 2.0 - 1.0);

		vWaves = SmoothTriangleWave(vWaves);
		float2 vWavesSum = vWaves.xz + vWaves.yw;
		vWavesSum.y = stiffness;

		//	Tumbling / Should be done before all other deformations
//#if defined (LEAFTUMBLING)

		// pos.w: upper bit = lodfade
		// Separate lodfade and twigPhase: lodfade stored in highest bit / twigphase compressed to 7 bits
		// moved to #ifs

		tumbleInfluence = frac(pos.w * 2.0);

		// Move point to 0,0,0
		pos.xyz -= pivot;

		float tumble = (_TumbleStrength == 0) ? 0 : 1;

		if ((_TumbleStrength || _LeafTurbulence /*> 0*/) && absWindStrength * tumbleInfluence > 0)
		{
			// _Wind.w is turbulence
			// Add variance to the different leaf planes

			// good for palms and bananas - but we do it later
			//	float3 fracs = frac( pivot * 33.3 + animParams.x * frac(fObjPhase) * 0.25 ); //fBranchPhase * 0.1); // + pos.w
			// good for trees	 	
			float3 fracs = frac(pivot * 33.3); //fBranchPhase * 0.1); // + pos.w
			float offset = fracs.x + fracs.y + fracs.z; ;
			float tFrequency = _TumbleFrequency * (_Time.y /* new */ + fObjPhase * 10);
			// Add different speeds: (1.0 + offset * 0.25)
			// float4 vWaves1 = SmoothTriangleWave( float4( (tFrequency + offset) * (1.0 + offset * 0.25), tFrequency * 0.75 - offset, tFrequency * 0.05 + offset, tFrequency * 1.5 + offset));
			// less sharp
			float4 vWaves1 = SmoothTriangleWave(float4((tFrequency + offset) * (1.0 + offset * 0.25), tFrequency * 0.75 + offset, tFrequency * 0.5 + offset, tFrequency * 1.5 + offset));
			// float4 vWaves1 = SmoothTriangleWave( float4( (tFrequency + offset), tFrequency * 0.75 - offset, tFrequency * 0.05 + offset, tFrequency * 2.5 + offset));
			float3 windDir = normalize(Wind.xyz);


//#if defined (_EMISSION)
			// This was the root of the fern issue: branchAxes slightly varied on different LODs!
			float3 branchAxis = frac(packedBranchAxis * float3(1.0f, 256.0f, 65536.0f));
			branchAxis = branchAxis * 2.0 - 1.0;
			branchAxis = normalize(branchAxis);
			// we can do better in case we have the baked branch main axis
			float facingWind = dot(branchAxis, windDir);
//#else
			//float facingWind = dot(normalize(float3(pos.x, 0, pos.z)), windDir); //saturate 
//#endif

			float3 windTangent = float3(-windDir.z, windDir.y, windDir.x);
			float twigPhase = vWaves1.x + vWaves1.y + (vWaves1.z * vWaves1.z);
			float windStrength = dot(abs(Wind.xyz), 1) * tumbleInfluence * (1.35 - facingWind) * Wind.w + absWindStrength; // Use abs(_Wind)!!!!!!

		//	turbulence
//#if defined (_EMISSION)
	// if(_LeafTurbulence) {
			float angle =
				// center rotation so the leaves rotate leftwards as well as rightwards according to the incoming waves
				// ((twigPhase + vWaves1.w + fBranchPhase) * 0.2 - 0.5) // not so good to add fBranchPhase here...
				((twigPhase + vWaves1.w) * 0.25 - 0.5)
				// make rotation strength depend on absWindStrength and all other inputs
				* 4.0 * absWindStrength * _LeafTurbulence * tumbleInfluence * (0.5 + animParams.w) * saturate(lerp(1.0, animParams.y * 8, _EdgeFlutterInfluence))
				;

			//branchAxis = normalize(branchAxis); // branch axis should be mostly normalized...
			float3x3 turbulenceRot = AfsRotationMatrix(-branchAxis, angle);
			pos.xyz = mul(turbulenceRot, pos.xyz);

#if defined(_NORMALMAP)
			_normal = mul(turbulenceRot, _normal.xyz);
#endif
			// #else
			//	pos.xyz = Rotate(pos.xyz, -branchAxis, angle);
			// #endif
		//}
//#endif

	//	tumbling
	// As used by the debug shader
//#if !defined (EFFECT_HUE_VARIATION)
	//if (_TumbleStrength) {
//				tumbleInfluence = frac(pos.w * 2.0);
				// + 1 is correct for trees/palm / -1 is correct for fern? allow negative values in the material inspector
			float angleTumble = (windStrength * (twigPhase + fBranchPhase * 0.25) * _TumbleStrength * tumbleInfluence * fOsc.y);

			// windTangent should be normalized
			float3x3 tumbleRot = AfsRotationMatrix(windTangent, angleTumble);
			pos.xyz = mul(tumbleRot, pos.xyz);

#if defined(_NORMALMAP)
			_normal = mul(tumbleRot, _normal.xyz);
#endif
			//#else
			//	pos.xyz = Rotate(pos.xyz, windTangent, angleTumble);
			//#endif
		//}
//#endif
		}

		//	crossfade – in case anybody uses it...
	//		#if defined(LOD_FADE_CROSSFADE)
	//			if (unity_LODFade.x != 0.0 && lodfade == 1.0) {
	//				pos.xyz *= unity_LODFade.x;
	//			}
	//		#endif
		//	fade in/out leave planes
#if defined(LOD_FADE_PERCENTAGE)
		//float lodfade = ceil(pos.w - 0.51);
		//float lodfade = (pos.w > 0.5) ? 1 : 0;
		float lodfade = (pos.w > (1.0f / 255.0f * 126.0f)) ? 1 : 0; // Make sure that the 1st vertex is taken into account
		//lodfade += _FadeOutAllLeaves;
		//if (/*unity_LODFade.x < 1.0 && */ lodfade) {
		pos.xyz *= 1.0 - unity_LODFade.x * lodfade;
		//}
#endif

// Move point back to origin
		pos.xyz += pivot;
//#endif

		//	Advanced edge fluttering (has to be outside preserve length)
#if defined(GEOM_TYPE_LEAF)
//#if !defined(IS_BARK)
		pos.xyz += _normal.xyz * SmoothTriangleWave(tumbleInfluence * _Time.y * _AdvancedEdgeBending.y + animParams.x) * _AdvancedEdgeBending.x * animParams.y * absWindStrength;
//#endif
#endif

		//	Preserve Length
		float origLength = length(pos.xyz);

		Wind.xyz *= fOsc.x;






		// Non Leaves
		//float3 bend = animParams.y * fDetailAmp * _normal.xyz * FastSign(_normal);
		
		// Leaves
		//float3 bend = float3(0, 0, 0);
		
		//bend.y = (animParams.y + animParams.w) * fBranchAmp;
		
		// Bark
		//float3 bend = float3(0, 0, 0);
		//bend.y = (animParams.w) * fBranchAmp;





		// Edge (xz) and branch bending (y)
//#if !defined(IS_BARK)
#if !defined(GEOM_TYPE_LEAF)
		float3 bend = animParams.y * fDetailAmp * _normal.xyz
//#if !defined(USE_VFACE)
			*FastSign(_normal)
//#endif
			;
#else
		float3 bend = float3(0, 0, 0);
#endif
		// Old style turbulence // bend.y = (animParams.w + animParams.y * _LeafTurbulence) * fBranchAmp;
		bend.y = (animParams.y + animParams.w) * fBranchAmp;

//#else
//		float3 bend = float3(0, 0, 0);
//		bend.y = (animParams.w) * fBranchAmp;
//#endif

		//	This gets never zero even if there is no wind. So we have to multiply it by length(Wind.xyz)
			// if not disabled in debug shader
//#if !defined(EFFECT_BUMP)
	// this is still fucking sharp!!!!!
	if (isLeavesRendering)
		pos.xyz += (((vWavesSum.xyx * bend) + (Wind.xyz * vWavesSum.y * animParams.w)) * Wind.w) * absWindStrength;
	else
		pos.xyz += (((vWavesSum.yyy * bend) + (Wind.xyz * vWavesSum.y * animParams.w)) * Wind.w) * absWindStrength;
//#endif

		//	Primary bending / Displace position
//#if !defined (ENABLE_WIND)
		pos.xyz += animParams.z * Wind.xyz;
//#endif

		//	Preserve Length
		pos.xyz = normalize(pos.xyz) * origLength;
		_vertex.xyz = pos.xyz;

		//	Store Variation
//#if !defined(UNITY_PASS_SHADOWCASTER) && defined (IS_LODTREE) && !defined (DEBUG)
//		_vertexColor.r = saturate((frac(TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) + frac((TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) * 3.3)) * 0.5);
//#endif

		//TouchBending(_vertex.xyz, texCoordY);
	}
}

void WindSimulationGrass(inout float3 vertex, float texCoordY)
{
	if (_WindState == 1)
	{
		//float _ShakeDisplacement = 10;
		//float factor = (1 - _ShakeDisplacement) * 0.5;
		//const float _WaveScale = _ShakeDisplacement;

		const float _WindSpeed = _ShakeTime - ((1 - _WindSpeedMultiplierGrass) * 0.001);
		const float4 _waveXSize = float4(0.048, 0.06, 0.24, 0.096);
		const float4 _waveZSize = float4(0.024, .08, 0.08, 0.2);
		const float4 waveSpeed = float4(1.2, 2, 1.6, 4.8);
		
		//float4 _waveXmove = float4(0.012, 0.02, -0.06, 0.048) * 10 * factor;
		//float4 _waveZmove = float4(0.006, .02, -0.02, 0.1) * 10 * factor;
		float4 _waveXmove = float4(0.024, 0.04, -0.12, 0.096);
		float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);
		
		float4 waves;
		waves = vertex.x * _waveXSize;
		waves += vertex.z * _waveZSize;
		
		waves += _Time.x * (1 - _ShakeWindspeed * 2) * waveSpeed * _WindSpeed;
		
		float4 s, c;
		waves = frac(waves);
		FastSinCos(waves, s, c);

		float waveAmount = texCoordY * _ShakeBending * clamp(_WindSpeedMultiplierGrass, 0, 1);
		s *= waveAmount;
		
		// Faster winds move the grass more than slow winds 
		s *= normalize(waveSpeed);
		
		s = s * s;
		float fade = dot(s, 1.3);
		s = s * s;
		float3 waveMove = float3 (0, 0, 0);
		waveMove.x = dot(s, _waveXmove);
		waveMove.z = dot(s, _waveZmove);
		vertex.xz -= mul((float3x3)unity_WorldToObject, waveMove).xz;

		//float noise = SimplexNoise(100 * float3(v.texcoord.xy, 0));
		
		//float2 animOffset1 = float2(0.f, 0.f);
		//float2 animOffset2 = vertex.xx * 1.f + vertex.zz * 0.5f;
		//animOffset2.x = (-1.f - pow(abs(sin(_Time.y / 0.75 + animOffset2.x)), 4.0f) - sin(_Time.y) * 0.05) * 0.025;
		//animOffset2.y = sin(_Time.y + animOffset2.y) * 0.05;
		//vertex.xz += animOffset2 * _WindState;
		//
		//half time = _Time.y;
		//half u = TexCoords(v).x;
		//half w1 = sin(_ShakeTime * _WindParam1.x - u * _WindParam1.y) * _WindParam1.z;
		//half w2 = sin(_ShakeTime * _WindParam2.x - u * _WindParam2.y) * _WindParam2.z;
		//vertex.y += normal * (w1 + w2) * u * 0.5 * _WindState;

		TouchBending(vertex, texCoordY);
	}
}

void WindSimulationGrass2 (inout float3 vertex, float2 texCoord, float4 normal)
{
	if (_WindState == 1)
	{
		//float _ShakeDisplacement = 10;
		//float factor = (1 - _ShakeDisplacement) * 0.5;
		//const float _WaveScale = _ShakeDisplacement;
		
		const float _WindSpeed = (_ShakeTime);
		
		const float4 _waveXSize = float4(0.048, 0.06, 0.24, 0.096);
		const float4 _waveZSize = float4 (0.024, .08, 0.08, 0.2);
		const float4 waveSpeed = float4 (1.2, 2, 1.6, 4.8);
		
		//float4 _waveXmove = float4(0.012, 0.02, -0.06, 0.048) * 10 * factor;
		//float4 _waveZmove = float4(0.006, .02, -0.02, 0.1) * 10 * factor;
		float4 _waveXmove = float4(0.024, 0.04, -0.12, 0.096);
		float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);
		
		float4 waves;
		waves = vertex.x * _waveXSize;
		waves += vertex.z * _waveZSize;
		
		waves += _Time.x * (1 - _ShakeWindspeed * 2) * waveSpeed * _WindSpeed;
		
		float4 s, c;
		waves = frac(waves);
		FastSinCos(waves, s, c);
		
		float waveAmount = texCoord.y * (_ShakeBending);
		s *= waveAmount;
		
		// Faster winds move the grass more than slow winds 
		s *= normalize(waveSpeed);
		
		s = s * s;
		float fade = dot(s, 1.3);
		s = s * s;
		float3 waveMove = float3 (0, 0, 0);
		waveMove.x = dot(s, _waveXmove);
		waveMove.z = dot(s, _waveZmove);
		vertex.xz -= mul((float3x3)unity_WorldToObject, waveMove).xz;
		
		//float2 animOffset1 = float2(0.f, 0.f);
		//float2 animOffset2 = vertex.xx * 1.f + vertex.zz * 0.5f;
		//animOffset2.x = (-1.f - pow(abs(sin(_Time.y / 0.75 + animOffset2.x)), 4.0f) - sin(_Time.y) * 0.05) * 0.025;
		//animOffset2.y = sin(_Time.y + animOffset2.y) * 0.05;
		//vertex.xz += animOffset2 * _WindState;
		//
		//half time = _Time.y;
		//half u = texCoord.x;
		//half w1 = sin(_ShakeTime * _WindParam1.x - u * _WindParam1.y) * _WindParam1.z;
		//half w2 = sin(_ShakeTime * _WindParam2.x - u * _WindParam2.y) * _WindParam2.z;
		//vertex.y += normal * (w1 + w2) * u * 0.5 * _WindState;

		TouchBending(vertex, texCoord.y);
	}
}

void WindSimulation (inout float4 _vertex, inout float3 _normal, float texCoordY)
{
	float3 pivot;
	float4 neutral = float4(0, 0, 0, 0);
	pivot.xz = (frac(float2(1.0f, 32768.0f) * neutral.xx) * 2) - 1;
	pivot.y = sqrt(1 - saturate(dot(pivot.xz, pivot.xz)));
	pivot *= neutral.y;
	float4 color = float4(4, 2, 0.03, 1);
	//float4 _TreeInstanceScale = float4(1, 1, 1, 1);
	//pivot *= _TreeInstanceScale.xyz;
	float4 TerrainLODWind = _TerrainLODWind;
	TerrainLODWind.xyz = mul((float3x3)unity_WorldToObject, _TerrainLODWind.xyz) * _WindSpeedMultiplier;

#if defined (_IS_LEAVES) // Leaves
	WindSimulationLeavesAndBark(true, float4(_vertex.xyz, color.b), float4(color.xy, neutral.xy), pivot, color.b, TerrainLODWind, neutral.z, _normal, _vertex, color, texCoordY);
#elif defined (_IS_GRASS) // Grass
	WindSimulationGrass(_vertex.xyz, texCoordY);
#elif defined (_IS_BARK) // Bark
	WindSimulationLeavesAndBark(false, float4(_vertex.xyz, color.b), float4(color.xy, neutral.xy), float3(0, 0, 0), 0, TerrainLODWind, 0, _normal, _vertex, color, texCoordY);
#else // If none of the settings are selected, then detect wind type from Rendering Type of the material
	#if defined (_ALPHATEST_ON) // Rendering Type - cutout, fade or transparent (Leaves)
			WindSimulationLeavesAndBark(true, float4(_vertex.xyz, color.b), float4(color.xy, neutral.xy), pivot, color.b, TerrainLODWind, neutral.z, _normal, _vertex, color, texCoordY);
	#else // Rendering Type - opaque (Bark)
			WindSimulationLeavesAndBark(false, float4(_vertex.xyz, color.b), float4(color.xy, neutral.xy), float3(0, 0, 0), 0, TerrainLODWind, 0, _normal, _vertex, color, texCoordY);
	#endif
#endif
}

#endif // TERRASTANDARD_WIND_CGINC_INCLUDED

