using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class MouseWheelHorizontalScroll : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.MouseWheel += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_delta().x == 0f)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					state.OffsetTimeArea((int)evt.get_delta().x * 10);
					result = base.ConsumeEvent();
				}
				return result;
			};
		}
	}
}
