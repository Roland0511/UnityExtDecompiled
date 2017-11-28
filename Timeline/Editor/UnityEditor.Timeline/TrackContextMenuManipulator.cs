using System;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TrackContextMenuManipulator : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.ContextClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				TimelineTrackGUI timelineTrackGUI = target as TimelineTrackGUI;
				bool result;
				if (timelineTrackGUI == null)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					if (!timelineTrackGUI.headerBounds.Contains(evt.get_mousePosition()))
					{
						bool flag = state.quadTree.GetItemsAtPosition(evt.get_mousePosition()).Any((IBounds x) => x is TimelineItemGUI);
						if (flag)
						{
							result = base.IgnoreEvent();
							return result;
						}
					}
					timelineTrackGUI.drawer.trackMenuContext.clipTimeCreation = TrackDrawer.TrackMenuContext.ClipTimeCreation.Mouse;
					timelineTrackGUI.drawer.trackMenuContext.mousePosition = evt.get_mousePosition();
					timelineTrackGUI.DisplayTrackMenu(state);
					result = base.ConsumeEvent();
				}
				return result;
			};
		}
	}
}
