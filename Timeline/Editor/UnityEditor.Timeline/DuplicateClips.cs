using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class DuplicateClips : ItemAction<TimelineClip>
	{
		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			PlayableDirector director = (state == null) ? null : state.currentDirector;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip clip = clips[i];
				TimelineClip timelineClip = clip.Duplicate(director);
				if (timelineClip != null && state != null)
				{
					SelectionManager.Clear();
					SelectionManager.Add(timelineClip);
				}
			}
			if (state != null)
			{
				state.Refresh();
			}
			return true;
		}
	}
}
