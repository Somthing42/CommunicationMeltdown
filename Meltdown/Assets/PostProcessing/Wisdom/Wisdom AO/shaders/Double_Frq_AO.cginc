uniform sampler2D _MainTex;
uniform sampler2D _CameraDepthNormalsTexture;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D noisetex;

uniform float maxDepth;

uniform float Radius;
uniform float Radius2;
uniform float intensity;

uniform float ColorBleedIntensity;

uniform float screenWidth;
uniform float screenHeight;

#include "lib/dither.cginc"

half dither;

#ifdef WAOColorBleed
half3 bleed = half3(0.0,0.0,0.0);

float colorEncode(half3 a) {
	uint3 v = uint3(round(saturate(a) * 255.0)) << uint3(0, 8, 16);
	
	return (v.r + v.g + v.b);
}
#endif

#include "AOV2.cginc"

float4 frag (v2f_img i) : COLOR {
	half2 uv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
	dither = find_closest(uv);

	half3 o1 = occulusion(uv, Radius);
	#ifdef WAOColorBleed
	half3 bleed1 = bleed;
	#endif
	half3 o2 = occulusion(uv, Radius2);
	o1.r *= o2.r;

	#ifdef WAOColorBleed
	return half4(o1, colorEncode(saturate(max(bleed1, bleed) * ColorBleedIntensity)));
	#else
	return half4(o1, 1.0);
	#endif
}