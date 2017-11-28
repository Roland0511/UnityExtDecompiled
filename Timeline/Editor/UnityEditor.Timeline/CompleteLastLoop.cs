using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Category("Editing/"), DisplayName("Complete Last Loop"), SeparatorMenuItem(SeparatorMenuItemPosition.Before)]
	internal class CompleteLastLoop : ItemAction<TimelineClip>
	{
		[CompilerGenerated]
		private static Func<TimelineClip, bool> <>f__mg$cache0;

		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			if (CompleteLastLoop.<>f__mg$cache0 == null)
			{
				CompleteLastLoop.<>f__mg$cache0 = new Func<TimelineClip, bool>(TimelineHelpers.HasUsableAssetDuration);
			}
			bool flag = clips.Any(CompleteLastLoop.<>f__mg$cache0);
			return (!flag) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return ClipModifier.CompleteLastLoop(clips);
		}
	}
}
