uniform sampler2D _MainTex;
uniform sampler2D _CameraDepthNormalsTexture;
half4 _MainTex_ST;
uniform half blurRadius;
uniform float maxDepth;

sampler2D_float _CameraDepthTexture;

half3 normalDecode(half2 encodedNormal) {
	encodedNormal = encodedNormal * 4.0 - 2.0;
	half f = dot(encodedNormal, encodedNormal);
	half g = sqrt(1.0 - f * 0.25);
	return half3(encodedNormal * g, 1.0 - f * 0.5);
}

half4 frag (v2f_img i) : COLOR {

	half2 iuv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
	float depth = Linear01Depth(tex2D (_CameraDepthTexture, iuv));
	if (depth > maxDepth) return half4(1.0, 1.0, 1.0, 1.0);

	// Center
	half3 s = tex2D(_MainTex, iuv).rgb;	
	half ao = s.r * 0.2941176f;

	half3 cNormal = normalDecode(s.gb);

	half2 offset1 = X(1.333333333f * blurRadius);

	// Left
	half2 uv = iuv - offset1;

	float3 n = tex2D (_MainTex, uv).rgb;
	half3 nNormal = normalDecode(n.gb);
	half CdotN = pow(max(0.0, dot(cNormal, nNormal)), 3.0);

	ao += lerp(s.r, n.r, pow(CdotN, 2.0)) * 0.352941176;

	// Right
	uv = iuv + offset1;
	
	n = tex2D (_MainTex, uv).rgb;
	nNormal = normalDecode(n.gb);
	CdotN = pow(max(0.0, dot(cNormal, nNormal)), 3.0);

	ao += lerp(s.r, n.r, pow(CdotN, 2.0)) * 0.352941176;


	return half4(ao, s.gb, 1.0);
}