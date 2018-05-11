// ======================================================================
//  Copyright 2017 Cheng Cao (Bob Cao, bobcaocheng@gmail.com)
//  All rights reserved. Do not modify/distribute it without permission
// ----------------------------------------------------------------------
//  If you bought this from Unity Assest Store, you have the permission
//  to use, modify, and distribute in binary format it on any kind of 
//  Unity3D program. This permission is limited to Unity3D platform.
// ======================================================================

#ifdef WAOSamples_Low
#define sampleDepth 1
#define Sample_Directions 6
#define SampleDirF 6.0

static const half2 offset_table[Sample_Directions + 1] = {
	half2( 0.0,    1.0 ),
	half2( 0.866,  0.5 ),
	half2( 0.866, -0.5 ),
	half2( 0.0,   -1.0 ),
	half2(-0.866, -0.5 ),
	half2(-0.866,  0.5 ),
	half2( 0.0,    1.0 )
};
#endif

#ifdef WAOSamples_Med
#define sampleDepth 2
#define Sample_Directions 6
#define SampleDirF 6.0

static const half2 offset_table[Sample_Directions + 1] = {
	half2( 0.0,    1.0 ),
	half2( 0.866,  0.5 ),
	half2( 0.866, -0.5 ),
	half2( 0.0,   -1.0 ),
	half2(-0.866, -0.5 ),
	half2(-0.866,  0.5 ),
	half2( 0.0,    1.0 )
};
#endif

#ifdef WAOSamples_High
#define sampleDepth 2
#define Sample_Directions 8
#define SampleDirF 8.0

static const half2 offset_table[Sample_Directions + 1] = {
	half2( 0.0,     1.0    ),
	half2( 0.7071,  0.7071 ),
	half2( 1.0,     0.0    ),
	half2( 0.7071, -0.7071 ),
	half2( 0.0,    -1.0    ),
	half2(-0.7071, -0.7071 ),
	half2(-1.0,     0.0    ),
	half2(-0.7071,  0.7071 ),
	half2( 0.0,     1.0    )
};
#endif

#ifdef WAOSamples_Ultra
#define sampleDepth 3
#define Sample_Directions 8
#define SampleDirF 8.0

static const half2 offset_table[Sample_Directions + 1] = {
	half2( 0.0,     1.0    ),
	half2( 0.7071,  0.7071 ),
	half2( 1.0,     0.0    ),
	half2( 0.7071, -0.7071 ),
	half2( 0.0,    -1.0    ),
	half2(-0.7071, -0.7071 ),
	half2(-1.0,     0.0    ),
	half2(-0.7071,  0.7071 ),
	half2( 0.0,     1.0    )
};
#endif

uniform float angleBias;
uniform float distanceBias;

uniform float scene_scale;

struct Frag {
	float depth;
	float3 wpos;
};

float3 cNormal;

float4x4 _InverseViewProject;
float4 _ProjInfoLeft;
float4 _ProjInfoRight;
uniform float4 _ProjInfo;

float4x4 _InvProj;

inline float3 ReconstructCSPosition(float2 S, float linearEyeZ) {
#ifdef UNITY_SINGLE_PASS_STEREO
	float4 projInfo = (unity_StereoEyeIndex == 0) ? _ProjInfoLeft : _ProjInfoRight;
	return float3((S.xy *  projInfo.xy +  projInfo.zw) * linearEyeZ, linearEyeZ);
#else
	return float3((S.xy * _ProjInfo.xy + _ProjInfo.zw) * linearEyeZ, linearEyeZ);
#endif
}

void init_frag (inout Frag f, in half2 uv) {
	f.depth = Linear01Depth(tex2D (_CameraDepthTexture, uv));

	f.wpos = ReconstructCSPosition(uv, f.depth);
}

uniform float aoexp;
uniform float Distribution;

half4 _MainTex_ST;

#ifdef WAO_USE_GENERATED_NORMAL
#define getNormal(a) half3(0.0,0.0,0.0)
#endif
#ifdef WAO_USE_GBUFFER_NORMAL
sampler2D _CameraNormalsTexture;
half3 getNormal(in half2 uv) {
	half3 n = mul((float3x3)unity_WorldToCamera, tex2D(_CameraNormalsTexture,uv) * 2.0 - 1.0);
	n.z = -n.z;
	return n;
}
#endif
#if !(defined(WAO_USE_GENERATED_NORMAL) || defined(WAO_USE_GBUFFER_NORMAL))
half3 getNormal(in half2 uv) {
	return DecodeViewNormalStereo (tex2D (_CameraDepthNormalsTexture, uv));
}
#endif

half2 normalEncode(fixed3 n) {
    return half2(n.xy * rsqrt(n.z * 8.0 + 8.0) + 0.5);
}

inline half3 occulusion(in half2 uv, in half R) {
	half am = 0.0;

	Frag fr;
	init_frag (fr, uv);

	if (fr.depth > maxDepth) return half4(1.0, 1.0, 1.0, 1.0);

	#ifdef WAO_USE_GENERATED_NORMAL
	cNormal = normalize(cross(ddx(fr.wpos), ddy(fr.wpos)));
	#else
	cNormal = getNormal(uv);
	#endif

	half d = R / (fr.depth * _ProjectionParams.z);

	half2 pixelBias = 2.0f / half2(screenWidth, screenHeight);
	#ifndef WAOSamples_Low
	half angleR = find_closest(uv);
	#else
	half angleR = 0.0;
	#endif

	#ifdef WAOColorBleed
	half3 W = normalize(fr.wpos);
	#endif

	for (int i = 0u; i < Sample_Directions; i++) {
		half dx = d * pow(series(dither, i), Distribution);
		half2 offset_t =
			   lerp(offset_table[i], offset_table[i+1], angleR)
			 * (dx + pixelBias);
			
		Frag sf;
		for (int j = 0u; j < sampleDepth; j++) {
			half2 uv1 = offset_t * (j + 1) / half(sampleDepth) + uv;

			init_frag (sf, uv1);

			half dist = distance(fr.wpos, sf.wpos) * scene_scale;
			half dist_att = dist / distanceBias;
			float diff = dot(cNormal, fr.wpos - sf.wpos);
			am +=
			    max(-angleBias, diff / dist)
			  * max(0.0, 1.0 - dist_att)
			  * float(dist > 0.000001);
		
			#ifdef WAOColorBleed
			if (diff > 0.001) {
				half3 color = tex2D(_MainTex, uv1).rgb;
				half3 H = normalize(W + normalize(fr.wpos-sf.wpos));

				bleed += max(dot(cNormal, H), 0.0) / pow(dist + 1.0, 2.0) * color;
			}
			#endif
		}
	}

	am = pow(saturate(1.0 - (am * intensity) / (SampleDirF * sampleDepth)), aoexp);
	#ifdef WAOColorBleed
	bleed /= (float) Sample_Directions * sampleDepth;
	#endif
	return half3(am, normalEncode(cNormal));
}