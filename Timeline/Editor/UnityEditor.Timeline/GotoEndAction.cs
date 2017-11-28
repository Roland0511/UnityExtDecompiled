using System;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("GotoEnd")]
	internal class GotoEndAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			bool result;
			if (state.IsEditingASubItem())
			{
				result = false;
			}
			else
			{
				state.time = state.duration;
				state.EnsurePlayHeadIsVisible();
				result = true;
			}
			return result;
		}
	}
}
