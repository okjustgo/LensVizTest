Shader "Diffuse with Vertex Colors" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.color.rgb;
			o.Alpha = IN.color.a;
			o.Specular = 0.3;
			o.Gloss = 0.5;
		}
		ENDCG
	}
	
	Fallback "VertexLit"
}