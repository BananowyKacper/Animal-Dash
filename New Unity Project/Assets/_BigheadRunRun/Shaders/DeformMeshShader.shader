// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/DeformMeshShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_ControlPointTexture("Control Point Texutre", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Geometry+1" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		int _Length;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		uniform sampler2D _ControlPointTexture;
		uniform float4 _DisplacementVer[200];

		float4 oldVerPos = 0;
		float minDis = -1;
		float4 displacement = 0;
		float4 originalVerPosTemp = 0;
		int i = 0;
		int minIndex = 0;
		struct Input {
			float2 uv_MainTex;
		};
		void vert(inout appdata_full v) {
			if (_Length > 0) {
				oldVerPos = v.vertex;
				minDis = -1;
				displacement = 0;
				originalVerPosTemp = oldVerPos;
				
				float4 indexColor = tex2Dlod(_ControlPointTexture, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				minIndex =  indexColor.x * 255;
				minDis = indexColor.w * 255;
				v.vertex.xyz += _DisplacementVer[minIndex];
				v.normal += _DisplacementVer[minIndex].xyz/20;
			}
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
		ENDCG
	}
	FallBack "Mobile/VertexLit"
}
