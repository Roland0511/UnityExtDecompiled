using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Category("Speed/"), DisplayName("Half Speed")]
	internal class HalfSpeed : ItemAction<TimelineClip>
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			bool flag = clips.All((TimelineClip x) => x.SupportsSpeedMultiplier());
			return (!flag) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return ClipModifier.HalfSpeed(clips);
		}
	}
}
