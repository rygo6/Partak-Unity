using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.CrashLog;

namespace Partak {
public class SystemSettings : MonoBehaviour {

	public bool FullVersion { get; set; }

	public int SessionCount { get; private set; }

	const string Version = "2.0.5";

	void Awake() {
		if (PlayerPrefs.HasKey("isFullVersion")) {
			Debug.Log("isFullVersion");
			FullVersion = true;	
		}

		SessionCount = PlayerPrefs.GetInt("SessionCount");
		Debug.Log("SessionCount: " + SessionCount);
		PlayerPrefs.SetInt("SessionCount", ++SessionCount);

		switch (PlayerPrefs.GetInt("muted")) {
		case 1:
			AudioListener.volume = 1f;
			break;
		case 2:
			AudioListener.volume = 0f;
			break;
		}

		CrashReporting.Init("ff1d2528-adf9-4ba4-bf2d-d34f2ccfe587", Version);
	}
}
}