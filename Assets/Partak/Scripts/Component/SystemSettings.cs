using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Partak
{
	public class SystemSettings : MonoBehaviour
	{
		public bool FullVersion { get; set; }

		public int SessionCount { get; private set; }

		private void Awake()
		{
			if (PlayerPrefs.HasKey("isFullVersion"))
			{
				Debug.Log("isFullVersion");
				FullVersion = true;	
			}

			SessionCount = PlayerPrefs.GetInt("SessionCount");
			Debug.Log("SessionCount: " + SessionCount);
			PlayerPrefs.SetInt("SessionCount", ++SessionCount);
		}

		private void Start()
		{
			if (SessionCount == 10)
				Persistent.Get<AnalyticsRelay>().SessionCount10();
		}
			
//		private void OnApplicationQuit()
//		{			
//			Debug.Log("Quitting on " + SceneManager.GetActiveScene().name);
//			Persistent.Get<AnalyticsRelay>().LevelQuit();
//			IncrementSessionCount();
//		}

		private void OnApplicationPause(bool pauseStatus)
		{
			Debug.Log("Pausing on " + SceneManager.GetActiveScene().name + " " + pauseStatus);
			Persistent.Get<AnalyticsRelay>().LevelQuit();
		}
	}
}