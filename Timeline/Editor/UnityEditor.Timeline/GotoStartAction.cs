using System;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("GotoStart")]
	internal class GotoStartAction : TimelineAction
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
				state.time = 0.0;
				state.EnsurePlayHeadIsVisible();
				result = true;
			}
			return result;
		}
	}
}
