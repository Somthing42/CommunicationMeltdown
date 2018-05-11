
uniform sampler2D _MainTex;
uniform sampler2D _CameraDepthNormalsTexture;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D noisetex;

uniform float maxDepth;

uniform float Radius;
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

	half3 ao = occulusion(uv, Radius);
	#ifdef WAOColorBleed
	return half4(ao, colorEncode(saturate(bleed * ColorBleedIntensity)));
	#else
	return half4(ao, 1.0);
	#endif
}