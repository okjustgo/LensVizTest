Shader "Custom/BlendVertexColors" {
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct VertexData {
				float4 position : POSITION;
				fixed4 color : COLOR;
			};

			struct FragData {
				float4 position : SV_POSITION;
				fixed4 color : COLOR;
			};

			FragData vert(VertexData v) {
				FragData o;
				o.position = mul(UNITY_MATRIX_MVP, v.position);
				o.color = v.color;
				return o;
			}

			fixed4 frag(FragData i) : SV_Target {
				return i.color;
			}
			ENDCG
		}
	}
}