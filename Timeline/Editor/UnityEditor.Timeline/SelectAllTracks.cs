using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class SelectAllTracks : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.KeyDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (EditorGUI.get_actionKey() && evt.get_keyCode() == 97)
				{
					SelectionManager.Clear();
					foreach (TimelineTrackBaseGUI current in TimelineWindow.instance.allTracks)
					{
						SelectionManager.Add(current.track);
					}
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
