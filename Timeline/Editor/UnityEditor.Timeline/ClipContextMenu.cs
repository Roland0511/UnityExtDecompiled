using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class ClipContextMenu : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.ContextClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				IEnumerable<TimelineClipGUI> source = SelectionManager.SelectedClipGUI();
				bool result;
				if (!source.Any<TimelineClipGUI>())
				{
					result = base.IgnoreEvent();
				}
				else if (!source.Any((TimelineClipGUI c) => c.bounds.Contains(evt.get_mousePosition())))
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineClipGUI timelineClipGUI = target as TimelineClipGUI;
					SequencerContextMenu.Show(timelineClipGUI.parentTrackGUI.drawer, evt.get_mousePosition());
					result = base.ConsumeEvent();
				}
				return result;
			};
		}
	}
}
