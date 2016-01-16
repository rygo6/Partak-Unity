// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/Diffuse VertexColor" {
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

struct Input {
    float3 color : COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	o.Albedo = IN.color.rgb;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
