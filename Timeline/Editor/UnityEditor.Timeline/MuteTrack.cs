using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Mute"), Shortcut("ToggleMute")]
	internal class MuteTrack : ToggleTrackAction
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			bool flag = (from x in tracks
			where x is GroupTrack
			select x).Any<TrackAsset>();
			MenuActionDisplayState result;
			if (flag)
			{
				result = MenuActionDisplayState.Hidden;
			}
			else
			{
				bool flag2 = (from x in tracks
				where !x.muted
				select x).Count<TrackAsset>() > 0;
				if (flag2)
				{
					result = MenuActionDisplayState.Visible;
				}
				else
				{
					result = MenuActionDisplayState.Hidden;
				}
			}
			return result;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			base.ToggleMute(state, tracks);
			return true;
		}
	}
}
