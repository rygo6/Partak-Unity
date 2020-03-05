Shader "GeoTetra/Highlight" {
	Properties{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, .1)) = .4
	}
	
    CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : POSITION;
	};

	uniform float _Outline;
	uniform float4 _OutlineColor;

	v2f vert(appdata v) {
        v2f o;   
        float dist = length(ObjSpaceViewDir(v.vertex));
        float4 newPos = v.vertex;
        float3 normal = normalize(v.normal);
        newPos += float4(normal, 0.0) * _Outline * dist;
        o.pos = UnityObjectToClipPos(newPos);
        return o;
	}
	ENDCG

	SubShader{
		Tags{ "Queue" = "Geometry" }

		Pass{
			Name "OUTLINE"
			Cull Front

			CGPROGRAM    
            #pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) :COLOR{
				return _OutlineColor;
			}
			ENDCG
		}
		
        Pass{
			Name "Overlay"
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) :COLOR {
				return _OutlineColor;
			}
			ENDCG
		}
	}
}