using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Partak.UI
{
	public class StartGame : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			StartCoroutine(LoadCoroutine());
		}

		private IEnumerator LoadCoroutine()
		{
			Prime31.EtceteraBinding.showActivityView();
			//done so sound can play
			yield return new WaitForSeconds(.5f);
			Application.LoadLevel("Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1));
		}
	}
}