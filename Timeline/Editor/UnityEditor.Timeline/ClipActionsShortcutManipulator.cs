using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class ClipActionsShortcutManipulator : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.KeyDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (state.IsEditingASubItem())
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineClipGUI timelineClipGUI = target as TimelineClipGUI;
					if (!SelectionManager.Contains(timelineClipGUI.clip))
					{
						result = base.IgnoreEvent();
					}
					else
					{
						result = ItemAction<TimelineClip>.HandleShortcut(state, evt, timelineClipGUI.clip);
					}
				}
				return result;
			};
		}
	}
}
