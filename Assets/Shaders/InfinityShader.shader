Shader "Custom/InfinityShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		#pragma target 3.0

		sampler2D _MainTex;
		
		float4x4 _Mtx, _Mtxi;
		float _T;

		struct Input {
			float2 uv_MainTex;
			float3 normal;
		};

		void vert (inout appdata_base v) {
			v.vertex.xy = mul(v.vertex, _Mtx).xy;
		}

		float a (float c, float h) {
			return c * cos(h);
		}

		float b (float c, float h) {
			return c * sin(h);
		}

		float3 oklab_to_linear_srgb (float3 lab) {
			float l_ = lab.x + 0.3963377774 * lab.y + 0.2158037573 * lab.z;
			float m_ = lab.x - 0.1055613458 * lab.y - 0.0638541728 * lab.z;
			float s_ = lab.x - 0.0894841775 * lab.y - 1.2914855480 * lab.z;

			float l = l_*l_*l_;
			float m = m_*m_*m_;
			float s = s_*s_*s_;

			return float3(
				+4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s,
				-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s,
				-0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s
			);
		}
		
//		const float Gamma = 1;
		float3 oklch_to_linear_rgb (float3 lch) {
			float3 col = oklab_to_linear_srgb(float3(lch.x, a(lch.y, lch.z), b(lch.y, lch.z)));
			return col;
//			return float3(pow(col.r, Gamma), pow(col.g, Gamma), pow(col.b, Gamma));
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = oklch_to_linear_rgb(float3(0.7, 0.2, _Time.z + 3.14159 * c.r));
			o.Emission = o.Albedo * 0.5;

			if (c.a < 0.1)
				discard;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
