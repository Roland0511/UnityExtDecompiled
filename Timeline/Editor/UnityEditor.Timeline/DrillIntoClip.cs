using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class DrillIntoClip : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.DoubleClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				TimelineClipGUI timelineClipGUI = target as TimelineClipGUI;
				bool result;
				if (timelineClipGUI == null || timelineClipGUI.clip == null)
				{
					result = base.IgnoreEvent();
				}
				else if (evt.get_button() != 0)
				{
					result = base.IgnoreEvent();
				}
				else if (!timelineClipGUI.rect.Contains(evt.get_mousePosition()))
				{
					result = base.IgnoreEvent();
				}
				else if (timelineClipGUI.clip.curves != null || timelineClipGUI.clip.animationClip != null)
				{
					ItemActionInvoker.InvokeByName<TimelineClip>("EditClipInAnimationWindow", state, timelineClipGUI.clip);
					result = base.ConsumeEvent();
				}
				else
				{
					result = base.IgnoreEvent();
				}
				return result;
			};
		}
	}
}
