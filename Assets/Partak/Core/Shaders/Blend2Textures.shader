Shader "Blend 2 Textures, Simply Lit" 
{ 
	
Properties 
{

	_Blend ("Blend", Range (0,1)) = 0.5 
	_Texture1 ("Texture 1", 2D) = "" 
	_Texture2 ("Texture 2", 2D) = ""
}

	Category 
	{

		SubShader 
		{
			Pass 
			{
		
				SetTexture[_Texture1]
				SetTexture[_Texture2] 
				{ 
					ConstantColor (0,0,0, [_Blend]) 
					Combine texture Lerp(constant) previous
				}
			}
		}
	}
}