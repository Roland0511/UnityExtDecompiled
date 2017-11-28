using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TrackDoubleClick : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.DoubleClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_button() != 0)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineTrackBaseGUI timelineTrackBaseGUI = target as TimelineTrackBaseGUI;
					result = EditTrackInAnimationWindow.Do(state, timelineTrackBaseGUI.track);
				}
				return result;
			};
		}
	}
}
