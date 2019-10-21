using UnityEngine;
using System.Collections;

namespace GeoTetra.GTSnapper
{
	[System.Serializable]
	public class ItemPartner
	{
		public string PrefabName { get; set; }
		public string[] TagArray { get; set; }
		public Item PartnerItemRaycast { get; set; }
	}
}