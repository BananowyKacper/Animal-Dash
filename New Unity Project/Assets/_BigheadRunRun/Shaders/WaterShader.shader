// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WaterShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_RandomTex("Random Texture", 2D) = "white" {}
		_Amount("Wave Strength",float) = 1
		_FogColor("Fog Color (RGB)", Color) = (0.5, 0.5, 0.5, 1.0)
		_FogStart("Fog Start", Float) = 0.0
		_FogEnd("Fog End", Float) = 10.0
		_Offset("offset",Range(0,1)) = 0
		_Range("Range",float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert finalcolor:fcolor addshadow 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#pragma multi_compile_fog


		sampler2D _MainTex;
		sampler2D _RandomTex;
		uniform float4 _CamPos;
		struct Input {
			float2 uv_MainTex;
			float fogVar;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Amount;
		fixed4 _FogColor;
		float _FogStart;
		float _FogEnd;
		float _Offset;
		float _Range;

		float4 getNewVertPosition(float4 p, inout appdata_full v)
		{
			float rand = tex2Dlod(_RandomTex, float4(v.texcoord.x, v.texcoord.y, 0, 0)).x;
			p.y += sin(_Time.y*rand*4  + rand)*_Amount /10;
			return p;
		}

		void vert(inout appdata_full v, out Input data) {
			data.uv_MainTex = v.texcoord.xy;
			float zpos = v.vertex.z/ _Range + _Offset;
			data.fogVar = min((_FogEnd - zpos) / (_FogEnd - _FogStart),1);
			float4 vertPosition = getNewVertPosition(v.vertex,v);
			float4 bitangent = float4(cross(v.normal, v.tangent), 0);
			float vertOffset = 0.01;
			float4 v1 = getNewVertPosition(v.vertex + v.tangent * vertOffset,v);
			float4 v2 = getNewVertPosition(v.vertex + bitangent * vertOffset,v);
			float4 newTangent = v1 - vertPosition;
			float4 newBitangent = v2 - vertPosition;
			v.normal = cross(newTangent, newBitangent)*100;
			v.vertex = vertPosition;
		}
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		void fcolor(in Input IN, SurfaceOutputStandard o, inout fixed4 color) {
			fixed3 fogColor = _FogColor.rgb;
			#ifndef UNITY_PASS_FORWARDBASE
			fogColor = 0;
			#endif
			color.rgb = lerp(color, fogColor,IN.fogVar);
		}


		ENDCG
	}
	FallBack "Diffuse"
}
