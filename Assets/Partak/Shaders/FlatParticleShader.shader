// Simplified Alpha Blended Particle shader. Differences from regular Alpha Blended Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Mobile/Particles/Flat" 
{	
Category {	
	Tags { "Queue"="Geometry" "IgnoreProjector"="True" "LightMode"="Always" }
	Lighting OFF
	ZWrite OFF
	Cull BACK
	
	BindChannels 
	{
		Bind "Color", color
	}	

	SubShader 
	{
		Pass 
		{
			Color [color] 
		}
	}
}
}
