using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal abstract class ToggleTrackAction : TrackAction
	{
		protected void ToggleLock(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			if (tracks.Length != 0)
			{
				TimelineUndo.PushUndo(tracks[0], "Lock Track");
				for (int i = 0; i < tracks.Length; i++)
				{
					TrackAsset trackAsset = tracks[i];
					trackAsset.locked = !trackAsset.locked;
				}
				state.Refresh(true);
			}
		}

		protected void ToggleMute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			if (tracks.Length != 0)
			{
				TimelineUndo.PushUndo(tracks[0], "Mute Track");
				for (int i = 0; i < tracks.Length; i++)
				{
					TrackAsset trackAsset = tracks[i];
					trackAsset.muted = !trackAsset.muted;
				}
				state.Refresh(true);
			}
		}
	}
}
