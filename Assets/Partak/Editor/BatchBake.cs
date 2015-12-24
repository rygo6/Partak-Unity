using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

public class BatchBake
{
	[MenuItem("Assets/Bake")]
	static public void Bake()
	{
		for (int i = 1; i < 19; ++i)
		{
			EditorSceneManager.OpenScene("Assets/Partak/Scenes/Level" + i + ".unity");
			Lightmapping.Bake();
			EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		}
	}
}
