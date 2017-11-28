using System;
using System.ComponentModel;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Category("Editing/"), DisplayName("Reset Editing")]
	internal class ResetClip : ItemAction<TimelineClip>
	{
		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return ClipModifier.ResetEditing(clips);
		}
	}
}
