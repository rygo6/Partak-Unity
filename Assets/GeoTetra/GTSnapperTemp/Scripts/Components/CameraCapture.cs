using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using UnityEngine;

#if  UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace GeoTetra.GTSnapper
{
	public class CameraCapture : SubscribableBehaviour
	{
		[SerializeField] private ServiceReference _componentContainer;
		[SerializeField] private Camera _camera;
		[SerializeField] private int _resWidth = 256;
		[SerializeField] private int _resHeight = 256;
		[SerializeField] private bool _saveOnStart;

		private void Awake()
		{
			if (_componentContainer.Asset != null) _componentContainer.Service<ComponentContainer>().RegisterComponent(this);
		}

		private void Start()
		{
#if  UNITY_EDITOR
			if (_saveOnStart)
			{
				string path = EditorSceneManager.GetActiveScene().path;
				path = path.Replace("unity", "png");
				SaveScreenshotToFile(path);
			}
#endif
		}

		private int CoordinateToGridIndex(int x, int y, int xMax, int yMax)
		{       
			if (x >= xMax || y >= yMax || x < 0 || y < 0)
			{
				return -1;
			}
			return ((y * xMax) + x);
		}

		private Rect CropRect(Texture2D texture)
		{
			int top = 0;
			int bottom = texture.height;
			int right = 0;
			int left = texture.width;
			Color[] pixels = texture.GetPixels();
			for (int y = 0; y < texture.height; ++y)
			{
				for (int x = 0; x < texture.width; ++x)
				{
					int index = CoordinateToGridIndex(x, y, texture.width, texture.height);

					if (pixels[index].a > .01f)
					{
						if (y > top)
						{
							top = y;
						}
						if (y < bottom)
						{
							bottom = y;
						}
						if (x > right)
						{
							right = x;
						}
						if (x < left)
						{
							left = x;
						}
					}
				}
			}

			if (right - left <= top - bottom)
			{
				return new Rect(left, bottom, right - left, top - bottom);			
			}

			return new Rect(left, left, right - left, right - left);
		}

		public void SaveScreenshotToFile(string fileName)
		{
			Debug.Log("Saving Screenshot " + fileName);
			
			RenderTexture rt = new RenderTexture(_resWidth, _resHeight, 32);
			
			_camera.clearFlags = CameraClearFlags.Color;
			_camera.backgroundColor = new Color(0,0,0, 0);
			_camera.targetTexture = rt;
			_camera.Render();

			RenderTexture.active = rt;

			Texture2D texture = new Texture2D(_resWidth, _resHeight, TextureFormat.ARGB32, false);
			texture.ReadPixels(new Rect(0, 0, _resWidth, _resHeight), 0, 0);

			_camera.targetTexture = null;
			RenderTexture.active = null; 
			
			Rect cropRect = CropRect(texture);
			Texture2D cropTexture = new Texture2D((int)cropRect.width, (int)cropRect.height, TextureFormat.ARGB32, false);
			Color[] cropPixels = texture.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height);
			cropTexture.SetPixels(cropPixels);	

			byte[] bytes = cropTexture.EncodeToPNG();
			if (System.IO.File.Exists(fileName))System.IO.File.Delete(fileName);
			System.IO.File.WriteAllBytes(fileName, bytes);
		}
	}
}