﻿Shader "Hidden/CameraEffect_DepthVerticalFog"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_FogDensity("Fog Density",Float) = 1
		_FogColor("Fog Color",Color) = (1,1,1,1)
		_FogVerticalStart("Fog Start",Float) = 0
		_FogVerticalOffset("Fog Offset",Float) = 1
		[NoScaleOffset]_NoiseTex("Noise Tex",2D) = "white"{}
		_NoiseScale("Noise Scale",float)=.5
		_NoiseSpeedX("Fog Speed Horizontal",Range(-.5,.5)) = .5
		_NoiseSpeedY("Fog Speed Vertical",Range(-.5,.5)) = .5
	}
		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma shader_feature _NOISE
				#include "../CommonInclude.hlsl"
				#include "CameraEffectInclude.hlsl"
				half _FogDensity;
				float _FogPow;
				float4 _FogColor;
				float _FogVerticalStart;
				float _FogVerticalOffset;
				#if _NOISE
				sampler2D _NoiseTex;
				float _NoiseScale;
				float _NoiseSpeedX;
				float _NoiseSpeedY;
				#endif
				struct v2f
				{
					float4 positionCS : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 uv_depth:TEXCOORD1;
					float3 viewDir:TEXCOORD2;
				};

				v2f vert (a2v_img v)
				{
					v2f o;
					o.positionCS = TransformObjectToHClip(v.positionOS);
					o.uv = v.uv;
					o.uv_depth = GetDepthUV(v.uv); 
					o.viewDir = GetInterpolatedRay(o.uv);
					return o;
				}

				float4 frag (v2f i) : SV_Target
				{
					float linearDepth = LinearEyeDepth(i.uv_depth);
					float3 worldPos = _WorldSpaceCameraPos+ i.viewDir.xyz*linearDepth;
					float2 worldUV = (worldPos.xz + worldPos.yz);
					float fog =  (( _FogVerticalStart+_FogVerticalOffset)-worldPos.y)  /_FogVerticalOffset*_FogDensity;
					#if _NOISE
					float2 noiseUV = worldUV / _NoiseScale + _Time.y*float2(_NoiseSpeedX,_NoiseSpeedY);
					float noise = tex2D(_NoiseTex, noiseUV).r;
					fog*=noise;
					#endif
					return lerp(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv) , _FogColor, saturate(fog));
				}
				ENDHLSL
		}
	}
}
