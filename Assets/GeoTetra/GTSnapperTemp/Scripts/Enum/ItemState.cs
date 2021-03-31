using UnityEngine;
using System.Collections;

namespace GeoTetra.GTSnapper
{
	public enum ItemState
	{
		Attached,
		AttachedHighlighted,
		Floating,
		Dragging,
		Instantiate,
		NoInstantiate,
	}
	
	public enum ItemHighlight
	{
		None,
		Highlighted
	}

	public enum ItemAction
	{
		ClickDown,
		ClickHold,
		ClickUp,
		BeginDrag,
		Drag,
		EndDrag,
		Drop,
	}
}