using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class NewTrackContextMenu : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.ContextClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (TimelineWindow.instance.sequenceHeaderBounds.Contains(evt.get_mousePosition()))
				{
					TimelineTrackBaseGUI[] visibleTrackGuis = TimelineWindow.instance.treeView.visibleTrackGuis;
					for (int i = 0; i < visibleTrackGuis.Length; i++)
					{
						TimelineTrackBaseGUI timelineTrackBaseGUI = visibleTrackGuis[i];
						Rect headerBounds = timelineTrackBaseGUI.headerBounds;
						headerBounds.set_y(headerBounds.get_y() + TimelineWindow.instance.treeviewBounds.get_y());
						if (headerBounds.Contains(evt.get_mousePosition()))
						{
							result = base.IgnoreEvent();
							return result;
						}
					}
					TimelineWindow.instance.ShowNewTracksContextMenu(null, null);
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
