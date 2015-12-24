using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Partak
{
	public class AnalyticsRelay : MonoBehaviour
	{
		public void SessionCount10()
		{
			Analytics.CustomEvent("SessionCount10", new Dictionary<string, object> {
				{ "FullVersion", Persistent.Get<SystemSettings>().FullVersion },
			});
		}

		public void GameTime(float gameTime)
		{
			Analytics.CustomEvent("GameTime_" + SceneManager.GetActiveScene().name, new Dictionary<string, object> {
				{ "Time", gameTime },
			});
		}

		public void GamePlayerCount()
		{
			Analytics.CustomEvent("GamePlayerCount", new Dictionary<string, object> {
				{ "PlayerCount", Persistent.Get<PlayerSettings>().ActivePlayerCount() },
			});
		}

		public void MenuLevelLoad()
		{
			string levelName = "Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1);
			Analytics.CustomEvent("MenuLeveLoad", new Dictionary<string, object> {
				{ "LevelName", levelName },
			});
			GamePlayerCount();
		}

		public void SkipLevel()
		{
			Analytics.CustomEvent("SkipLevel", new Dictionary<string, object> {
				{ "LevelName", SceneManager.GetActiveScene().name },
			});
			GamePlayerCount();
		}

		public void ReplayLevel()
		{
			Analytics.CustomEvent("ReplayLevel", new Dictionary<string, object> {
				{ "LevelName", SceneManager.GetActiveScene().name },
			});
			GamePlayerCount();
		}

		public void LevelQuit()
		{
			Analytics.CustomEvent("LevelQuit", new Dictionary<string, object> {
				{ "LevelName", SceneManager.GetActiveScene().name },
			});
		}
	}
}