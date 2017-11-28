using System;
using System.ComponentModel;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Category("Editing/"), DisplayName("Match Duration")]
	internal class MatchDuration : ItemAction<TimelineClip>
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return (clips.Length <= 1) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return ClipModifier.MatchDuration(clips);
		}
	}
}
