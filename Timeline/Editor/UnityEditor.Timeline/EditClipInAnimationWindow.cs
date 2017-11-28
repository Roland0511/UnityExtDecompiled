using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Edit in Animation Window"), SeparatorMenuItem(SeparatorMenuItemPosition.After)]
	internal class EditClipInAnimationWindow : ItemAction<TimelineClip>
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			MenuActionDisplayState result;
			if (clips.Length == 1 && clips[0].animationClip != null)
			{
				result = MenuActionDisplayState.Visible;
			}
			else
			{
				result = MenuActionDisplayState.Hidden;
			}
			return result;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			TimelineClip timelineClip = clips[0];
			bool result;
			if (timelineClip.curves != null || timelineClip.animationClip != null)
			{
				AnimationClip animationClip = (!(timelineClip.animationClip != null)) ? timelineClip.curves : timelineClip.animationClip;
				if (animationClip == null)
				{
					result = false;
				}
				else
				{
					Component bindingForTrack = state.GetBindingForTrack(timelineClip.parentTrack);
					TimelineWindowTimeControl timeController = TimelineAnimationUtilities.CreateTimeController(state, timelineClip);
					TimelineAnimationUtilities.EditAnimationClipWithTimeController(animationClip, timeController, (!(timelineClip.animationClip != null)) ? null : bindingForTrack);
					result = true;
				}
			}
			else
			{
				result = false;
			}
			return result;
		}
	}
}
