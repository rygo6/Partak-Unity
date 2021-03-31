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
	
	public enum ItemAction
	{
		Down,
		Up,
		BeginDrag,
		Drag,
		EndDrag,
	}
}