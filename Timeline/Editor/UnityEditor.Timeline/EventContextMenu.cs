using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class EventContextMenu : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.ContextClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				IEnumerable<TimelineMarkerGUI> source = SelectionManager.SelectedMarkerGUI();
				bool result;
				if (!source.Any<TimelineMarkerGUI>())
				{
					result = base.IgnoreEvent();
				}
				else if (!source.Any((TimelineMarkerGUI c) => c.bounds.Contains(evt.get_mousePosition())))
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineMarkerGUI timelineMarkerGUI = target as TimelineMarkerGUI;
					SequencerContextMenu.Show(timelineMarkerGUI.parentTrackGUI.drawer, evt.get_mousePosition());
					result = base.ConsumeEvent();
				}
				return result;
			};
		}
	}
}
