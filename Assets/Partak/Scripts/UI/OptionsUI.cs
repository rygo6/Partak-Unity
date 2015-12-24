using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections;

namespace Partak
{
	public class OptionsUI : MonoBehaviour
	{
		public void RestorePurchases()
		{
			Persistent.Get<Store>().RestorePurchases();
		}
	}
}