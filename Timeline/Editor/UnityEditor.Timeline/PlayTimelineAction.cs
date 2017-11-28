using System;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("Play")]
	internal class PlayTimelineAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			bool playing = state.playing;
			TimelineWindow.instance.Simulate(!playing);
			state.playing = !playing;
			return true;
		}
	}
}
