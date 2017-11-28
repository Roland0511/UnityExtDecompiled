using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class DeleteTracks : TrackAction
	{
		public static void Do(TimelineAsset timeline, TrackAsset track)
		{
			TrackModifier.DeleteTrack(timeline, track);
		}

		public override bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			for (int i = 0; i < tracks.Length; i++)
			{
				TrackAsset track = tracks[i];
				DeleteTracks.Do(state.timeline, track);
			}
			state.previewMode = false;
			state.Refresh();
			return true;
		}
	}
}
