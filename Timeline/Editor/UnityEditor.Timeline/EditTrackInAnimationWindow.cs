using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Edit in Animation Window"), SeparatorMenuItem(SeparatorMenuItemPosition.After)]
	internal class EditTrackInAnimationWindow : TrackAction
	{
		public static bool Do(TimelineWindow.TimelineState state, TrackAsset track)
		{
			AnimationTrack animationTrack = track as AnimationTrack;
			bool result;
			if (animationTrack == null)
			{
				result = false;
			}
			else if (!animationTrack.CanConvertToClipMode())
			{
				result = false;
			}
			else
			{
				Component bindingForTrack = state.GetBindingForTrack(animationTrack);
				TimelineWindowTimeControl timeController = TimelineAnimationUtilities.CreateTimeController(state, EditTrackInAnimationWindow.CreateTimeControlClipData(animationTrack));
				TimelineAnimationUtilities.EditAnimationClipWithTimeController(animationTrack.animClip, timeController, (!(bindingForTrack != null)) ? null : bindingForTrack.get_gameObject());
				result = true;
			}
			return result;
		}

		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			MenuActionDisplayState result;
			if (tracks.Length == 0)
			{
				result = MenuActionDisplayState.Hidden;
			}
			else
			{
				if (tracks[0] is AnimationTrack)
				{
					AnimationTrack track = tracks[0] as AnimationTrack;
					if (track.CanConvertToClipMode())
					{
						result = MenuActionDisplayState.Visible;
						return result;
					}
				}
				result = MenuActionDisplayState.Hidden;
			}
			return result;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			return EditTrackInAnimationWindow.Do(state, tracks[0]);
		}

		private static TimelineWindowTimeControl.ClipData CreateTimeControlClipData(AnimationTrack track)
		{
			return new TimelineWindowTimeControl.ClipData
			{
				track = track,
				start = (float)track.start,
				duration = (float)track.get_duration()
			};
		}
	}
}
