using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Partak.UI
{
	public class PauseButton : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			FindObjectOfType<CellParticleEngine>().Pause = true;
		}
	}
}