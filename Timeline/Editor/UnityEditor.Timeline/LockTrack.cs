using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Lock"), Shortcut("ToggleLock")]
	internal class LockTrack : ToggleTrackAction
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			bool flag = tracks.Any((TrackAsset x) => x is GroupTrack);
			MenuActionDisplayState result;
			if (flag)
			{
				result = MenuActionDisplayState.Hidden;
			}
			else
			{
				bool flag2 = tracks.Any((TrackAsset x) => !x.locked);
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
			base.ToggleLock(state, tracks);
			return true;
		}
	}
}
