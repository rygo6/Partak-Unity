// Unlit shader. Simplest possible textured shader.
// - SUPPORTS lightmap
// - no lighting
// - no per-material color

Shader "Mobile/Unlit (Supports Lightmap) Blend 2" {
Properties {
	_Blend ("Blend", Range (0,1)) = 0.5 
	_MainTex ("Base (RGB)", 2D) = ""
	_Texture2 ("Texture 2", 2D) = ""
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	// Non-lightmapped
	Pass {
		Tags { "LightMode" = "Vertex" }
		Lighting Off
		SetTexture [_MainTex] {
			constantColor (1,1,1,1)
			combine texture, constant // UNITY_OPAQUE_ALPHA_FFP
		}  
		SetTexture[_Texture2] 
		{ 
			ConstantColor (0,0,0, [_Blend]) 
			Combine texture Lerp(constant) previous
		}
	}
	
	// Lightmapped, encoded as dLDR
	Pass {
		Tags { "LightMode" = "VertexLM" }

		Lighting Off
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			combine texture
		}
		SetTexture [_MainTex] {
			constantColor (1,1,1,1)
			combine texture * previous DOUBLE, constant // UNITY_OPAQUE_ALPHA_FFP
		}
		SetTexture[_Texture2] 
		{ 
			ConstantColor (0,0,0, [_Blend]) 
			Combine texture Lerp(constant) previous
		}
	}
	
	// Lightmapped, encoded as RGBM
	Pass {
		Tags { "LightMode" = "VertexLMRGBM" }
		
		Lighting Off
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			combine texture * texture alpha DOUBLE
		}
		SetTexture [_MainTex] {
			constantColor (1,1,1,1)
			combine texture * previous QUAD, constant // UNITY_OPAQUE_ALPHA_FFP
		}
		SetTexture[_Texture2] 
		{ 
			ConstantColor (0,0,0, [_Blend]) 
			Combine texture Lerp(constant) previous
		}
	}	
	

}
}



