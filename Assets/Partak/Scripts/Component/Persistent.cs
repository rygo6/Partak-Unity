using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Partak
{
	public class Persistent : MonoBehaviour
	{
		static private Persistent _instance;

		private readonly Dictionary<Type, Component> ComponentDictionary = new Dictionary<Type, Component>();

		static private void Init()
		{
			GameObject prefab = (GameObject)Resources.Load("Persistent");
			GameObject instance = Instantiate(prefab);
			_instance = instance.GetComponent<Persistent>();
			DontDestroyOnLoad(_instance);
		}

		static public T Get<T>() where T : Component
		{
			if (_instance == null)
				Init();

			Component component;
			_instance.ComponentDictionary.TryGetValue(typeof(T), out component);
			if (component != null)
			{
				return (T)component;
			}
			else
			{
				component = _instance.GetComponentInChildren<T>();
				_instance.ComponentDictionary.Add(typeof(T), component);
				return (T)component;
			}
		}
	}
}
