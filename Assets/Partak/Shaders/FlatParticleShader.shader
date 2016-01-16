Shader "Mobile/Particles/Flat" 
{	
Category {	

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
