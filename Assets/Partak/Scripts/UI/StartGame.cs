using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Analytics;
using System.Collections.Generic;

namespace Partak.UI
{
	public class StartGame : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			PlayerSettings playerSettings = Persistent.Get<PlayerSettings>();
			int activeCount = 0;
			for (int i = 0; i < PlayerSettings.MaxPlayers; ++i)
			{
				if (playerSettings.GetPlayerMode(i) != PlayerMode.None)
					activeCount++;
			}
			if (activeCount >= 2)
			{
				StartCoroutine(LoadCoroutine());
			}
			else
			{
				Prime31.EtceteraBinding.showAlertWithTitleMessageAndButtons(
					"Enable More Players", 
					"Game needs atleast two players set to Human or Comp to start game.", 
					new string[1]{"Ok"});
			}
		}

		private IEnumerator LoadCoroutine()
		{
			string levelName = "Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1);
			Analytics.CustomEvent("MenuLeveLoad", new Dictionary<string, object>
			{
				{"LevelName", levelName},
			});

			Prime31.EtceteraBinding.showActivityView();
			//done so sound can play
			yield return new WaitForSeconds(.5f);
					Application.LoadLevel(levelName);
		}
	}
}