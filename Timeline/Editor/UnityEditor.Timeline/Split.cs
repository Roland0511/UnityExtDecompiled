using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Category("Editing/"), Shortcut("Split")]
	internal class Split : ItemAction<TimelineClip>
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return (!clips.All((TimelineClip x) => state.time <= x.start || state.time >= x.start + x.duration)) ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			bool flag = ClipModifier.Split(state.currentDirector, state.time, clips);
			if (flag)
			{
				state.Refresh();
			}
			return flag;
		}
	}
}
