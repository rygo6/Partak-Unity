using UnityEngine;
using System.Collections;

namespace Partak
{
	public class Persistent : MonoBehaviour 
	{
		static private Persistent _instance;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			_instance = this;
		}

		static public T Get<T>()
		{
			return _instance.GetComponentInChildren<T>();
		}
	}
}
