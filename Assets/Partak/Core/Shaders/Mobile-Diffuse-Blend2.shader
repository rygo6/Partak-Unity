// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/Diffuse Blend2" {
Properties {
	_Blend ("Blend", Range (0,1)) = 0.5 
	_MainTex ("Base (RGB)", 2D) = ""
	_Texture2 ("Texture 2", 2D) = ""
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
sampler2D _Texture2;
float _Blend;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 c2 = tex2D(_Texture2, IN.uv_MainTex);
	fixed4 output = lerp(c, c2, _Blend);
	o.Albedo = output.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
