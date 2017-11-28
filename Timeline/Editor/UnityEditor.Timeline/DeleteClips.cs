using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class DeleteClips : ItemAction<TimelineClip>
	{
		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			state.Stop();
			ClipModifier.Delete(state.timeline, clips);
			SelectionManager.Clear();
			state.Refresh(true);
			return true;
		}
	}
}
