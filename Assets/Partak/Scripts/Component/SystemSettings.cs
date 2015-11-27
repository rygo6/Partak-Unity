using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;

namespace Partak
{
	public class SystemSettings : MonoBehaviour
	{
		public bool FullVersion { get; set; }

		private const string WelcomeFullMessage = "Partak is a complete rewrite and rebrand of the game priorly known as enrgy. This is the first step of a new effort to revamp and improve this game, expect more updates. As someone who purchased the Full Version of enrgy, know that you will always recieve a fully featured, unlocked and ad-free version of Partak. Thank you for your early support.";

		private const string WelcomeMessage = "Partak is a complete rewrite and rebrand of the game priorly known as enrgy. This is the first step of a new effort to revamp and improve this game, expect more updates.";

		private void Awake()
		{
			if (PlayerPrefs.HasKey("isFullVersion") && !PlayerPrefs.HasKey("WelcomeMessage"))
			{
				FullVersion = true;	
				Prime31.EtceteraBinding.showAlertWithTitleMessageAndButtons("Welcome to Partak", WelcomeFullMessage, new string[1]{ "Ok" });
				PlayerPrefs.SetInt("WelcomeMessage", 1);
			}
			else if (!PlayerPrefs.HasKey("WelcomeMessage"))
			{
				Prime31.EtceteraBinding.showAlertWithTitleMessageAndButtons("Welcome to Partak", WelcomeMessage, new string[1]{ "Ok" });
				PlayerPrefs.SetInt("WelcomeMessage", 1);
			}
		}

		private void OnApplicationQuit()
		{			
			Debug.Log("Quitting on " + Application.loadedLevelName);
			LogLevelQuit();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			Debug.Log("Pausing on " + Application.loadedLevelName + " " + pauseStatus);
			LogLevelQuit();
		}

		private void LogLevelQuit()
		{
			Analytics.CustomEvent("LevelQuit", new Dictionary<string, object>
			{
				{"LevelName", Application.loadedLevelName},
			});
		}
	}
}