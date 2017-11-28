using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TrackZoom : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.MouseWheel += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (EditorGUI.get_actionKey())
				{
					state.trackScale = Mathf.Min(Mathf.Max(state.trackScale + evt.get_delta().y * 0.1f, 1f), 100f);
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
