using System;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("NextFrame")]
	internal class NextFrameAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			state.frame++;
			return true;
		}
	}
}
