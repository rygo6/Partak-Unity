using UnityEngine;
using UnityEditor;
using System.Collections;

public class BatchBake
{
	[MenuItem("Assets/Bake")]
	static public void Bake()
	{
		for (int i = 11; i < 19; ++i)
		{
			EditorApplication.OpenScene("Assets/Partak/Scenes/Level" + i + ".unity");
			Lightmapping.Bake();
			EditorApplication.SaveScene();
		}
	}
}
