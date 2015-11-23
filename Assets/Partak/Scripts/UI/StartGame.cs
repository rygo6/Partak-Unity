using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Partak.UI
{
	public class StartGame : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			Application.LoadLevel("Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1));
		}
	}
}