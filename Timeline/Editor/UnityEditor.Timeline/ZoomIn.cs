using System;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("ZoomIn")]
	internal class ZoomIn : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			TimelineZoomManipulator.DoZoom(2f, state);
			return true;
		}
	}
}
