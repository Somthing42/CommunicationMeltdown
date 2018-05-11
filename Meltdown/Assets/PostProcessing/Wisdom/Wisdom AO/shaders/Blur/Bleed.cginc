uniform sampler2D _MainTex;
uniform sampler2D _CameraDepthNormalsTexture;
half4 _MainTex_ST;
uniform half blurRadius;
uniform float maxDepth;

uniform float scene_scale;

sampler2D_float _CameraDepthTexture;

half3 normalDecode(half2 encodedNormal) {
	encodedNormal = encodedNormal * 4.0 - 2.0;
	half f = dot(encodedNormal, encodedNormal);
	half g = sqrt(1.0 - f * 0.25);
	return half3(encodedNormal * g, 1.0 - f * 0.5);
}

half3 unpackColor(float f) {
	uint3 decode = uint3(f,f,f);
    decode.yz = decode.yz >> uint2(8, 16);
    decode.xyz = decode.xyz & 255;
	
	return half3(decode) / 255.0;
}

float colorEncode(half3 a) {
	uint3 v = uint3(round(saturate(a) * 255.0)) << uint3(0, 8, 16);
	
	return (v.r + v.g + v.b);
}

float4 frag (v2f_img i) : COLOR {

	half2 iuv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
	float depth = Linear01Depth(tex2D (_CameraDepthTexture, iuv));
	if (depth > maxDepth) return half4(1.0, 1.0, 1.0, 1.0);

	// Center
	float4 s = tex2D(_MainTex, iuv);	
	half ao = s.r * 0.2941176f;
	
	half3 center_col = unpackColor(s.a);
	half3 col = center_col * 0.2941176f;

	half3 cNormal = normalDecode(s.gb);

	half2 offset1 = X(blurRadius);

	// Left
	half2 uv = iuv - offset1;

	float4 n = tex2D (_MainTex, uv);
	half3 nNormal = normalDecode(n.gb);
	half CdotN = pow(max(0.0, dot(cNormal, nNormal)), 3.0);

	ao += lerp(s.r, n.r, pow(CdotN, 2.0)) * 0.352941176;
	col += lerp(center_col, unpackColor(n.a), pow(CdotN, 2.0)) * 0.352941176;

	// Right
	uv = iuv + offset1;
	
	n = tex2D (_MainTex, uv);
	nNormal = normalDecode(n.gb);
	CdotN = pow(max(0.0, dot(cNormal, nNormal)), 3.0);

	ao += lerp(s.r, n.r, pow(CdotN, 2.0)) * 0.352941176;
	col += lerp(center_col, unpackColor(n.a), pow(CdotN, 2.0)) * 0.352941176;

	return float4(ao, s.gb, colorEncode(saturate(col)));
}