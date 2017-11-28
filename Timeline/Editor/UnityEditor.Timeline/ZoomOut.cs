using System;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("ZoomOut")]
	internal class ZoomOut : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			TimelineZoomManipulator.DoZoom(-2f, state);
			return true;
		}
	}
}
