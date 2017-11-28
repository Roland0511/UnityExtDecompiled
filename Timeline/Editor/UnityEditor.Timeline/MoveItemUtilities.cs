using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class MoveItemUtilities
	{
		public static bool IsDriver(TimelineItemGUI target, Event evt, TimelineWindow.TimelineState state)
		{
			return SelectionManager.Contains(target.item);
		}
	}
}
