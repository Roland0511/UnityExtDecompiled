using System;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("PrevFrame")]
	internal class PreviousFrameAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			state.frame--;
			return true;
		}
	}
}
