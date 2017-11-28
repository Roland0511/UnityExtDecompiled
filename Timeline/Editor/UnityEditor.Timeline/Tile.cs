using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class Tile : ItemAction<TimelineClip>
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return (clips.Length <= 1) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return ClipModifier.Tile(clips);
		}
	}
}
