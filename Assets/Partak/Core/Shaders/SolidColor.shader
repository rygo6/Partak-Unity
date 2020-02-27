Shader "Solid Color" 
{
    Properties 
    {
        _Color ("Main Color", Color) = (1,1,1,1)
    }

	Category 
	{	
	    SubShader 
	    {
	        Pass { Color [_Color] }
	    }
	}
}