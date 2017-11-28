using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Category("Editing/"), DisplayName("Trim End"), Shortcut("TrimEnd")]
	internal class TrimEnd : ItemAction<TimelineClip>
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return (!clips.All((TimelineClip x) => state.time <= x.start || state.time >= x.start + x.duration)) ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return ClipModifier.TrimEnd(state.time, clips);
		}
	}
}
