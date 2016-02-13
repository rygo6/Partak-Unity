using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Partak
{
	public class AnalyticsRelay : MonoBehaviour
	{
		int _levelPlayCount;

		void OnApplicationPause(bool pauseStatus)
		{
			LevelQuit();
			LevelPlayCount();
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
				{ "PlayerCount", Persistent.Get<MenuConfig>().ActivePlayerCount() },
			});
			Analytics.CustomEvent("HumanPlayerCount", new Dictionary<string, object> {
				{ "PlayerCount", Persistent.Get<MenuConfig>().ActiveHumanPlayerCount() },
			});
		}

		public void MenuLevelLoad()
		{
			string levelName = "Level" + (Persistent.Get<MenuConfig>().LevelIndex + 1);
			Analytics.CustomEvent("MenuLeveLoad", new Dictionary<string, object> {
				{ "LevelName", levelName },
			});
			++_levelPlayCount;
			GamePlayerCount();
		}

		public void NextLevel()
		{
			++_levelPlayCount;
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
			++_levelPlayCount;
			GamePlayerCount();
		}

		void LevelPlayCount()
		{
			Analytics.CustomEvent("LevelPlayCount", new Dictionary<string, object> {
				{ "count", _levelPlayCount },
			});
		}

		void LevelQuit()
		{
			Analytics.CustomEvent("LevelQuit", new Dictionary<string, object> {
				{ "LevelName", SceneManager.GetActiveScene().name },
			});
		}
	}
}