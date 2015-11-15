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

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			if (_instance == null)
			{
				_instance = this;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		static public T Get<T>() where T : Component
		{
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
