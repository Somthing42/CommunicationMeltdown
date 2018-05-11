// ======================================================================
//  Copyright 2017 Cheng Cao (Bob Cao, bobcaocheng@gmail.com)
//  All rights reserved. Do not modify/distribute it without permission
// ----------------------------------------------------------------------
//  If you bought this from Unity Assest Store, you have the permission
//  to use, modify, and distribute in binary format it on any kind of 
//  Unity3D program. This permission is limited to Unity3D platform.
// ======================================================================

Shader "Hidden/WisdomAOV2"
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		WAO_rtAO ("Base (RGB)", 2D) = "" {}
		WAO_blurTex ("Base (RGB)", 2D) = "" {}
		WAO_InjectFrame ("Base (RGB)", 2D) = "" {}
	}
	SubShader {

	CGINCLUDE
	
		#pragma exclude_renderers flash
		#pragma target 3.0
		#include "UnityCG.cginc"

	ENDCG

	// ===================================================================
	//  AO V2 - HBAO method
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile WAOSamples_Low WAOSamples_Med WAOSamples_High WAOSamples_Ultra
			#pragma shader_feature WAOColorBleed
			#include "shaders/Single_Frq_AO.cginc"

			ENDCG
		}

	// ===================================================================
	//  Blur Pass (Advanced), Pass 1, Horizontal
	// ===================================================================
	//  Coding language: CG
	//  default vertex kernel, fragment kernel -> frag
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#define X(a) half2(a, 0.0f)
			#include "shaders/blur/Advanced.cginc"

			ENDCG
		}

	// ===================================================================
	//  Blur Pass (Bleed), Pass 2
	// ===================================================================
	//  Coding language: CG
	//  default vertex kernel, fragment kernel -> frag
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#define X(a) half2(a, 0.0f)
			#include "shaders/blur/Bleed.cginc"

			ENDCG
		}

	// ===================================================================
	//  Composite Pass, Pass 3
	// ===================================================================
	//  Coding language: CG
	//  default vertex kernel, fragment kernel -> frag
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#pragma shader_feature WAOColorBleed

			sampler2D _MainTex;
			half4 _MainTex_ST;

			sampler2D WAO_rtAO;

			#ifdef WAOColorBleed
			half3 unpackColor(float f) {
				uint3 decode = uint3(f,f,f);
			    decode.yz = decode.yz >> uint2(8, 16);
			    decode.xyz = decode.xyz & 255;
	
				return half3(decode) / 255.0;
			}
			#endif

			half4 frag(v2f_img i) : SV_Target {
				half2 iuv = UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST);
				half4 c = tex2D (_MainTex, iuv);
				#ifdef WAOColorBleed
				float2 t = tex2D (WAO_rtAO, iuv).ra;
				c.rgb *= t.r;
				c.rgb += unpackColor(t.y);
				#else
				c.rgb *= tex2D (WAO_rtAO, iuv).r;
				#endif
				return c;
			}

			ENDCG
		}

	// ===================================================================
	//  Non-composite AO, Pass 4
	// ===================================================================
	//  Coding language: CG
	//  default vertex kernel, fragment kernel -> frag
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#pragma shader_feature WAOColorBleed

			uniform sampler2D WAO_rtAO;
			half4 _MainTex_ST;

			#ifdef WAOColorBleed
			half3 unpackColor(float f) {
				uint3 decode = uint3(f,f,f);
			    decode.yz = decode.yz >> uint2(8, 16);
			    decode.xyz = decode.xyz & 255;
	
				return half3(decode) / 255.0;
			}
			#endif

			float4 frag (v2f_img i) : COLOR {
				#ifdef WAOColorBleed
				float2 ao = tex2D(WAO_rtAO, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST)).ra;
				half3 col = unpackColor(ao.y) + half3(0.5,0.5,0.5) * ao.r;
				return half4(col, 1.0);
				#else
				half ao = tex2D(WAO_rtAO, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST)).r;
				return half4(ao,ao,ao,1.0);
				#endif
			}

			ENDCG
		}

	// ===================================================================
	//  AO V2 - HBAO method - double frequency
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile WAOSamples_Low WAOSamples_Med WAOSamples_High WAOSamples_Ultra
			#pragma shader_feature WAOColorBleed
			#include "shaders/Double_Frq_AO.cginc"

			ENDCG
		}


	// ===================================================================
	//  AO V2 - HBAO method - Generated Normal
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile WAOSamples_Low WAOSamples_Med WAOSamples_High WAOSamples_Ultra
			#pragma shader_feature WAOColorBleed
			#define WAO_USE_GENERATED_NORMAL
			#include "shaders/Single_Frq_AO.cginc"
			
			ENDCG
		}

	// ===================================================================
	//  AO V2 - HBAO method - double frequency
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile WAOSamples_Low WAOSamples_Med WAOSamples_High WAOSamples_Ultra
			#pragma shader_feature WAOColorBleed
			#define WAO_USE_GENERATED_NORMAL
			#include "shaders/Double_Frq_AO.cginc"

			ENDCG
		}

	// ===================================================================
	//  Blur Pass (Advanced), Pass 1, Verticle
	// ===================================================================
	//  Coding language: CG
	//  default vertex kernel, fragment kernel -> frag
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#define X(a) half2(0.0f, a)
			#include "shaders/blur/Advanced.cginc"

			ENDCG
		}

	// ===================================================================
	//  AO V2 - HBAO method - Gbuffer Normal
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile WAOSamples_Low WAOSamples_Med WAOSamples_High WAOSamples_Ultra
			#pragma shader_feature WAOColorBleed
			#define WAO_USE_GBUFFER_NORMAL
			#include "shaders/Single_Frq_AO.cginc"

			ENDCG
		}

	// ===================================================================
	//  AO V2 - HBAO method - double frequency - Gbuffer Normal
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile WAOSamples_Low WAOSamples_Med WAOSamples_High WAOSamples_Ultra
			#pragma shader_feature WAOColorBleed
			#define WAO_USE_GBUFFER_NORMAL
			#include "shaders/Double_Frq_AO.cginc"

			ENDCG
		}

	// ===================================================================
	//  Blur Pass (Bleed), Pass 2
	// ===================================================================
	//  Coding language: CG
	//  default vertex kernel, fragment kernel -> frag
	// ===================================================================
		Pass {
		ZTest Always Cull Off ZWrite Off
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#define X(a) half2(0.0f, a)
			#include "shaders/blur/Bleed.cginc"

			ENDCG
		}
	}

}