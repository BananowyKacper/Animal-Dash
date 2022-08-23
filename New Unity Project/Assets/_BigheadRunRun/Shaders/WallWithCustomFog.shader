// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WallWithCustomFog" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_FogColor("FogColor", Color) = (1,1,1,1)
		_FogMaxDis("FogMaxDistance", float ) = 10
		_FogMinDis("FogMinDistance", float ) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert finalcolor:fcolor

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			fixed3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _FogColor;
		fixed _FogMaxDis;
		fixed _FogMinDis;
		void vert(inout appdata_full v, out Input data) {
			fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
			data.worldPos = worldPos;
			data.uv_MainTex = v.texcoord.xy;
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void fcolor(in Input IN, SurfaceOutputStandard o, inout fixed4 color) {
			fixed fogVar = distance(IN.worldPos, _WorldSpaceCameraPos);
			fixed3 fogColor = _FogColor.rgb;
			fixed fogvarTemp = min(1 - min((fogVar- _FogMinDis)/(_FogMaxDis- _FogMinDis),1),1);
			#ifndef UNITY_PASS_FORWARDBASE
				fogColor = 0;
			#endif
			color.rgb = lerp(color, fogColor,fogvarTemp);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
