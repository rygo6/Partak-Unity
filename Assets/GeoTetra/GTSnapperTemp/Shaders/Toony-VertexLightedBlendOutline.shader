Shader "GeoTetra/VertexLightedBlendOutline" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 8)) = 1
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Blend ("Blend (RGB)", 2D) = "white" {}				
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Overlay"}
		UsePass "Mobile/VertexLit (Only Directional Lights)/FORWARD"
		UsePass "GeoTetra/OutlineOnly/OUTLINE"
	} 
}
