using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Unlock")]
	internal class UnlockTrack : ToggleTrackAction
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			bool flag = (from x in tracks
			where !x.locked
			select x).Count<TrackAsset>() > 0;
			MenuActionDisplayState result;
			if (flag)
			{
				result = MenuActionDisplayState.Hidden;
			}
			else
			{
				result = MenuActionDisplayState.Visible;
			}
			return result;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			base.ToggleLock(state, tracks);
			return true;
		}
	}
}
