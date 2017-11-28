using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TrackShortcutManipulator : Manipulator
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
					TimelineTrackBaseGUI trackGUI = target as TimelineTrackBaseGUI;
					if (trackGUI == null || trackGUI.track == null)
					{
						result = base.IgnoreEvent();
					}
					else
					{
						bool arg_AC_0;
						if (!SelectionManager.SelectedTracks().Contains(trackGUI.track))
						{
							arg_AC_0 = (from x in SelectionManager.SelectedItemGUI()
							select x).Any((TimelineItemGUI x) => x.parentTrackGUI == trackGUI);
						}
						else
						{
							arg_AC_0 = true;
						}
						bool flag = arg_AC_0;
						if (flag)
						{
							result = TrackAction.HandleShortcut(state, evt, trackGUI.track);
						}
						else
						{
							result = base.IgnoreEvent();
						}
					}
				}
				return result;
			};
		}
	}
}
