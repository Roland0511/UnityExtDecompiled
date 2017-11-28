using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("NudgeRight")]
	internal class NudgeRightAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			bool result;
			if (state.IsEditingASubItem())
			{
				result = false;
			}
			else if (SelectionManager.Count() == 0)
			{
				result = false;
			}
			else
			{
				bool flag = false;
				foreach (TimelineClip current in SelectionManager.SelectedItems<TimelineClip>())
				{
					flag |= TimelineHelpers.NudgeClip(current, state, 1.0);
				}
				if (flag)
				{
					state.Evaluate();
				}
				result = true;
			}
			return result;
		}
	}
}
